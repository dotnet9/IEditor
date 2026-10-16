const { chromium }=require('playwright');const path=require('path');const fs=require('fs');const {pathToFileURL}=require('url');
(async()=>{
 const root=path.resolve(__dirname,'../../..');const output=process.env.IEDITOR_AUDIT_OUTPUT||path.join(root,'artifacts/prototype-audit');fs.mkdirSync(output,{recursive:true});
 const productVersion=fs.readFileSync(path.join(root,'src/IEditor.App/IEditor.App.csproj'),'utf8').match(/<Version>(\d+\.\d+\.\d+)<\/Version>/)?.[1];
 const isolated=path.join(output,'offline-copy');fs.mkdirSync(isolated,{recursive:true});const html=path.join(isolated,'index.html');fs.copyFileSync(path.join(root,'design/index.html'),html);
 const browser=await chromium.launch({headless:true,...(process.env.CI?{}:{channel:'msedge'})});const context=await browser.newContext({viewport:{width:1320,height:860},deviceScaleFactor:1,offline:true});
 const page=await context.newPage();const errors=[];const external=[];const primaryColors=[];page.on('pageerror',e=>errors.push(e.stack));page.on('request',request=>{if(/^https?:/.test(request.url()))external.push(request.url());});
 let count=0;for(const route of ['photo','batch','settings']){
  await page.goto(pathToFileURL(html).href+'?capture=1#'+route);await page.reload();
  await page.waitForFunction(()=>!!document.querySelector('iframe').contentWindow.IEditorPrototype);
  const frame=page.frames().find(frame=>frame!==page.mainFrame());
  // srcdoc 页继承外层查询参数之前，显式启用产品窗口截图视口。
  await frame.evaluate(()=>document.body.dataset.capture='true');
  const nativeSwatches=await frame.locator('.title-palette-swatches button').evaluateAll(items=>items.map(item=>({key:item.dataset.paletteKey,color:item.style.getPropertyValue('--swatch')})));
  if(JSON.stringify(nativeSwatches.map(item=>item.key))!==JSON.stringify(['DotNet','Aurora','Ember','Default','Aquatic','Desert','Dusk','NightSky']))throw Error('Title palette order differs from the native enum');
  if(!await frame.evaluate(items=>items.every(item=>item.color===window.IEDITOR_PALETTES[item.key].Light.StudioAccentColor),nativeSwatches))throw Error('Title palette colors differ from native light preview brushes');
  for(const language of ['zh-CN','en-US'])for(const palette of ['DotNet','Aurora','Ember','Default','Aquatic','Desert','Dusk','NightSky'])for(const mode of ['Light','Dark']){
   await frame.evaluate(state=>window.IEditorPrototype.set(state),{language,palette,mode});
   const state=await frame.evaluate(()=>window.IEditorPrototype.getState());if(state.language!==language||state.palette!==palette||state.mode!==mode)throw Error('State failed '+route);
   if(await frame.locator('.title-palette-swatches [data-palette-key]').count()!==8||await frame.locator('.product-tools select').count()!==0)throw Error('Compact appearance tools missing');
   if(await frame.locator('[data-theme-cycle]').getAttribute('data-mode')!==mode)throw Error('Theme button state');
   if(JSON.stringify(await frame.locator('.title-palette-swatches button').evaluateAll(items=>items.map(item=>({key:item.dataset.paletteKey,color:item.style.getPropertyValue('--swatch')}))))!==JSON.stringify(nativeSwatches))throw Error('Title palette colors changed with the theme');
   if(!await frame.locator('main').innerText())throw Error('Empty workspace');
   if(palette==='DotNet'){
    const file=route+'-'+language+'-'+mode;await page.screenshot({path:path.join(output,file+'.png'),animations:'disabled'});
    const geometry=await frame.evaluate(()=>{const result=['region,x,y,width,height'];for(const [key,selector]of Object.entries({'ie-topbar':'.topbar','ie-statusbar':'.statusbar','ie-stage':'.stage','ie-card':'.card','ie-card-header':'.card-h','ie-row':'.row'})){let index=0;for(const element of document.querySelectorAll(selector)){const rect=element.getBoundingClientRect();result.push([key+'-'+index++,rect.x,rect.y,rect.width,rect.height].join(','));}}return result.join('\n')+'\n';});
    fs.writeFileSync(path.join(output,file+'.geometry.csv'),geometry);
   }count++;
  }
  if(route==='photo'){
   await frame.locator('.menu').first().hover();await frame.locator('[data-export]').first().click();if(!await frame.locator('#exportDialog').isVisible())throw Error('Export did not open');
   await verifyPrimary(frame,frame.locator('#exportDialog .btn.primary'),'Export');
   await frame.locator('#qRange').evaluate(el=>{el.value='80';el.dispatchEvent(new Event('input',{bubbles:true}));});if(await frame.locator('#qVal').innerText()!=='80')throw Error('Quality did not change');await frame.locator('#exportDialog [data-close]').first().click();
   await frame.locator('#btnGuides').click();if(!await frame.locator('#guides').evaluate(el=>el.classList.contains('hidden')))throw Error('Guide toggle failed');
  }
  // 逐项实际点击，首项位于菜单栏上方，可发现标题栏层叠上下文遮挡。
  const menuCultures=await frame.locator('.title-language-menu [data-culture]').evaluateAll(items=>items.map(item=>item.dataset.culture));
  if(menuCultures.length!==2||!menuCultures.includes('zh-CN')||!menuCultures.includes('en-US'))throw Error('Language choices differ from the application');
  for(const culture of [...menuCultures,...menuCultures]){
   await frame.locator('[data-language-open]').click();if(!await frame.locator('.title-language-menu').isVisible())throw Error('Language menu did not open: '+route);
   await frame.locator('[data-culture="'+culture+'"]').click();
   if(await frame.locator('.title-language-menu').isVisible())throw Error('Language menu did not close: '+route);
   if((await frame.evaluate(()=>window.IEditorPrototype.getState())).language!==culture)throw Error('Language menu did not switch the existing workspace: '+route+'/'+culture);
  }
  const previousMode=await frame.locator('[data-theme-cycle]').getAttribute('data-mode');for(let index=0;index<3;index++)await frame.locator('[data-theme-cycle]').click();if(await frame.locator('[data-theme-cycle]').getAttribute('data-mode')!==previousMode)throw Error('Three-state theme cycle failed');
  if(await frame.locator('.titlebar .win-controls button').count()!==3)throw Error('Missing desktop caption buttons');
  if(await frame.locator('.titlebar .win-controls > button[data-window-action]').count()!==3||await frame.locator('[data-theme-cycle][data-window-action]').count())throw Error('Caption actions must belong to the three desktop buttons');
  await frame.locator('[data-window-action="maximize"]').click();if(!await frame.locator('.window').evaluate(el=>el.classList.contains('prototype-maximized')))throw Error('Maximize did not change the window frame');
  await frame.locator('[data-window-action="maximize"]').click();if(await frame.locator('.window').evaluate(el=>el.classList.contains('prototype-maximized')))throw Error('Restore did not reset the window frame');
  for(const action of ['minimize','close']){await frame.locator('[data-window-action="'+action+'"]').click();if(await frame.locator('.window').isVisible()||!await frame.locator('[data-window-restore]').isVisible())throw Error('Window action did not hide the workspace: '+route+'/'+action);await frame.locator('[data-window-restore]').click();if(!await frame.locator('.window').isVisible())throw Error('Window restore did not reopen the workspace');}
  await frame.evaluate(()=>window.IEditorPrototype.openAbout());if(!await frame.locator('.about-titlebar').isVisible()||!await frame.locator('.about-meta').innerText().then(text=>text.includes('2026-10-15 00:00:00')))throw Error('About desktop chrome or sample build time missing');
  if(route==='photo')await verifyPrimary(frame,frame.locator('[data-update-action]'),'About');
  for(const language of ['zh-CN','en-US']){
   await frame.evaluate(state=>window.IEditorPrototype.set(state),{language,mode:'Light',palette:'DotNet'});
   for(const state of ['checking','available','downloading','downloaded','canceled','failed']){await frame.evaluate(value=>window.IEditorPrototype.setUpdate(value),state);if(!await frame.locator('[data-update-action]').innerText())throw Error('Missing update action');}
   await frame.locator('.product-about').screenshot({path:path.join(output,'about-'+language+'.png'),animations:'disabled'});
  }if(!await frame.locator('.about-meta').innerText().then(text=>text.includes('v'+productVersion)))throw Error('Prototype and product versions differ');await frame.locator('.about-close').click();
  if(route==='batch'){
   await page.setViewportSize({width:1100,height:760});
   for(const language of ['zh-CN','en-US']){
    await frame.evaluate(state=>window.IEditorPrototype.set(state),{language,mode:'Light',palette:'DotNet'});
    await frame.locator('.batch-options').evaluate(element=>element.scrollTop=element.scrollHeight);
    const reachable=await frame.locator('.batch-options .switch').last().evaluate(element=>{const point=element.getBoundingClientRect(),panel=element.closest('.batch-options').getBoundingClientRect();return point.top>=panel.top&&point.bottom<=panel.bottom;});
    if(!reachable)throw Error('Minimum-window batch option is clipped: '+language);
    const original=await frame.locator('.batch-options input[type=checkbox]').last().isChecked();
    await frame.locator('.batch-options .switch').last().click();
    if(await frame.locator('.batch-options input[type=checkbox]').last().isChecked()===original)throw Error('Minimum-window option click did not update its value');
    await page.screenshot({path:path.join(output,'batch-minimum-'+language+'.png'),animations:'disabled'});
   }
   await page.setViewportSize({width:1320,height:860});
  }
  const remaining=await frame.evaluate(()=>{const result=[];const walker=document.createTreeWalker(document.querySelector('.window'),NodeFilter.SHOW_TEXT);let node;while(node=walker.nextNode())if(!node.parentElement.closest('script,style,svg,select')&&node.parentElement.checkVisibility()&&/[\u4e00-\u9fff]/.test(node.textContent))result.push(node.textContent.trim());return [...new Set(result)].filter(Boolean);});
  fs.writeFileSync(path.join(output,route+'-remaining-chinese.json'),JSON.stringify(remaining,null,2));
  if(remaining.length)throw Error('Untranslated visible content in '+route+': '+remaining.join('; '));
 }
 fs.writeFileSync(path.join(output,'primary-button-colors.json'),JSON.stringify(primaryColors,null,2));
 await browser.close();if(errors.length||external.length)throw Error([...errors,...external].join('\n'));console.log('PASS '+count+' workspace/language/palette/mode combinations; '+primaryColors.length+' primary button state records; export and guide interactions; About/update states; isolated offline HTML; zero JavaScript errors or external requests.');
 async function verifyPrimary(frame,button,action){
  await frame.addStyleTag({content:'*,*::before,*::after{transition:none!important;animation:none!important}'});
  for(const palette of ['DotNet','Aurora','Ember','Default','Aquatic','Desert','Dusk','NightSky'])for(const mode of ['Light','Dark']){
   await frame.evaluate(state=>window.IEditorPrototype.set(state),{language:'zh-CN',palette,mode});
   await page.mouse.move(1,1);await record('normal','--button-primary');
   await button.hover();await record('hover','--button-primary-hover');
   await page.mouse.down();await record('pressed','--button-primary-pressed');
   await page.mouse.move(1,1);await page.mouse.up();
   await button.evaluate(element=>element.disabled=true);await record('disabled',null);
   await button.evaluate(element=>element.disabled=false);await record('reenabled','--button-primary');
   async function record(state,key){
    const value=await button.evaluate((element,key)=>{const style=getComputedStyle(element);const rgb=value=>value.match(/[\d.]+/g).slice(0,3).map(Number);const lum=value=>rgb(value).map(channel=>{const x=channel/255;return x<=.04045?x/12.92:((x+.055)/1.055)**2.4;}).reduce((sum,x,index)=>sum+x*[.2126,.7152,.0722][index],0);const a=lum(style.color),b=lum(style.backgroundColor);const probe=document.createElement('i');probe.style.background=getComputedStyle(document.documentElement).getPropertyValue(key||'--button-primary');document.body.append(probe);const expected=getComputedStyle(probe).backgroundColor;probe.remove();return{foreground:style.color,background:style.backgroundColor,expected,ratio:(Math.max(a,b)+.05)/(Math.min(a,b)+.05),opacity:Number(style.opacity),disabled:element.disabled};},key);
    primaryColors.push({action,palette,mode,state,...value});
    if(key&&(value.background!==value.expected||value.ratio<4.5))throw Error('Unreadable primary control: '+JSON.stringify({action,palette,mode,state,...value}));
    if(!key&&(!value.disabled||value.opacity>=1))throw Error('Missing disabled primary control: '+action);
   }
  }
 }
})().catch(error=>{console.error(error);process.exit(1);});
