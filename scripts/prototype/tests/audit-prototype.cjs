const { chromium }=require('playwright');const path=require('path');const fs=require('fs');const {pathToFileURL}=require('url');
(async()=>{
 const root=path.resolve(__dirname,'../../..');const output=process.env.IEDITOR_AUDIT_OUTPUT||path.join(root,'artifacts/prototype-audit');fs.mkdirSync(output,{recursive:true});
 const isolated=path.join(output,'offline-copy');fs.mkdirSync(isolated,{recursive:true});const html=path.join(isolated,'index.html');fs.copyFileSync(path.join(root,'design/index.html'),html);
 const browser=await chromium.launch({headless:true,...(process.env.CI?{}:{channel:'msedge'})});const context=await browser.newContext({viewport:{width:1320,height:860},deviceScaleFactor:1,offline:true});
 const page=await context.newPage();const errors=[];const external=[];page.on('pageerror',e=>errors.push(e.stack));page.on('request',request=>{if(/^https?:/.test(request.url()))external.push(request.url());});
 let count=0;for(const route of ['photo','batch','settings']){
  await page.goto(pathToFileURL(html).href+'?capture=1#'+route);await page.reload();
  await page.waitForFunction(()=>!!document.querySelector('iframe').contentWindow.IEditorPrototype);
  const frame=page.frames().find(frame=>frame!==page.mainFrame());
  // srcdoc 页继承外层查询参数之前，显式启用产品窗口截图视口。
  await frame.evaluate(()=>document.body.dataset.capture='true');
  for(const language of ['zh-CN','en-US'])for(const palette of ['DotNet','Aurora','Ember','Default','Aquatic','Desert','Dusk','NightSky'])for(const mode of ['Light','Dark']){
   await frame.evaluate(state=>window.IEditorPrototype.set(state),{language,palette,mode});
   const state=await frame.evaluate(()=>window.IEditorPrototype.getState());if(state.language!==language||state.palette!==palette||state.mode!==mode)throw Error('State failed '+route);
   if(await frame.locator('.product-tools [data-palette] option').count()!==8||await frame.locator('.product-tools [data-mode] option').count()!==3)throw Error('Appearance options missing');
   if(!await frame.locator('main').innerText())throw Error('Empty workspace');
   if(palette==='DotNet'){
    const file=route+'-'+language+'-'+mode;await page.screenshot({path:path.join(output,file+'.png'),animations:'disabled'});
    const geometry=await frame.evaluate(()=>{const result=['region,x,y,width,height'];for(const [key,selector]of Object.entries({'ie-topbar':'.topbar','ie-statusbar':'.statusbar','ie-stage':'.stage','ie-card':'.card','ie-card-header':'.card-h','ie-row':'.row'})){let index=0;for(const element of document.querySelectorAll(selector)){const rect=element.getBoundingClientRect();result.push([key+'-'+index++,rect.x,rect.y,rect.width,rect.height].join(','));}}return result.join('\n')+'\n';});
    fs.writeFileSync(path.join(output,file+'.geometry.csv'),geometry);
   }count++;
  }
  if(route==='photo'){
   await frame.locator('.menu').first().hover();await frame.locator('[data-export]').first().click();if(!await frame.locator('#exportDialog').isVisible())throw Error('Export did not open');
   await frame.locator('#qRange').evaluate(el=>{el.value='80';el.dispatchEvent(new Event('input',{bubbles:true}));});if(await frame.locator('#qVal').innerText()!=='80')throw Error('Quality did not change');await frame.locator('#exportDialog [data-close]').first().click();
   await frame.locator('#btnGuides').click();if(!await frame.locator('#guides').evaluate(el=>el.classList.contains('hidden')))throw Error('Guide toggle failed');
  }
  if(await frame.locator('.titlebar .win-controls button').count()!==3)throw Error('Missing desktop caption buttons');
  await frame.evaluate(()=>window.IEditorPrototype.openAbout());if(!await frame.locator('.about-titlebar').isVisible()||!await frame.locator('.about-meta').innerText().then(text=>text.includes('2026-10-15 00:00:00')))throw Error('About desktop chrome or sample build time missing');for(const language of ['zh-CN','en-US']){
   await frame.evaluate(state=>window.IEditorPrototype.set(state),{language,mode:'Light',palette:'DotNet'});
   for(const state of ['checking','available','downloading','downloaded','canceled','failed']){await frame.evaluate(value=>window.IEditorPrototype.setUpdate(value),state);if(!await frame.locator('[data-update-action]').innerText())throw Error('Missing update action');}
   await frame.locator('.product-about').screenshot({path:path.join(output,'about-'+language+'.png'),animations:'disabled'});
  }await frame.locator('.about-close').click();
  const remaining=await frame.evaluate(()=>{const result=[];const walker=document.createTreeWalker(document.querySelector('.window'),NodeFilter.SHOW_TEXT);let node;while(node=walker.nextNode())if(!node.parentElement.closest('script,style,svg,select')&&/[\u4e00-\u9fff]/.test(node.textContent))result.push(node.textContent.trim());return [...new Set(result)].filter(Boolean);});
  fs.writeFileSync(path.join(output,route+'-remaining-chinese.json'),JSON.stringify(remaining,null,2));
  if(remaining.length)throw Error('Untranslated visible content in '+route+': '+remaining.join('; '));
 }
 await browser.close();if(errors.length||external.length)throw Error([...errors,...external].join('\n'));console.log('PASS '+count+' workspace/language/palette/mode combinations; export and guide interactions; About/update states; isolated offline HTML; zero JavaScript errors or external requests.');
})().catch(error=>{console.error(error);process.exit(1);});
