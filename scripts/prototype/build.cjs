// 将三个工作区与所有样式、脚本、图标写入独立 HTML；生成结果可复制后离线打开。
const fs=require('fs');const path=require('path');
const root=path.resolve(__dirname,'../..');const source=path.join(__dirname,'source');const output=path.join(root,'design/index.html');
const check=process.argv.includes('--check');
const asset=name=>fs.readFileSync(path.join(source,name),'utf8').replace(/^\uFEFF/,'').replace(/\r\n/g,'\n');
const image=name=>'data:image/'+(name.endsWith('.svg')?'svg+xml':'png')+';base64,'+fs.readFileSync(path.join(root,name)).toString('base64');
const pages={};for(const route of ['photo','batch','settings']){
 let html=asset(route+'.html');
 html=html.replace(/<link rel="stylesheet" href="([^"]+)">/g,(_,file)=>'<style>'+asset(file)+'</style>');
 html=html.replace(/<script src="([^"]+)"><\/script>/g,(_,file)=>'<script>'+asset(file)+'</script>');
 html=html.replace(/(?:\.\.\/)+logo\.(svg|png)/g,(_,extension)=>image('logo.'+extension));
 const aliases={'证件照片处理.html':'photo','证件照处理.html':'photo','批量处理.html':'batch','设置.html':'settings'};
 for(const [filename,target]of Object.entries(aliases))html=html.replaceAll('href="'+filename+'"','href="#'+target+'"');
 html=html.replace('</body>',`<script>document.addEventListener('click',event=>{const anchor=event.target.closest('a[href]');if(anchor&&['#photo','#batch','#settings'].includes(anchor.getAttribute('href'))){event.preventDefault();parent.postMessage({kind:'ieditor-route',route:anchor.getAttribute('href').slice(1)},'*');}});<\/script></body>`);
 pages[route]=html;
}
const json=JSON.stringify(pages).replaceAll('</','<\\/');
const html=`<!doctype html>
<html lang="zh-CN"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>IEditor · 产品交互原型</title><link rel="icon" href="${image('logo.svg')}"><style>html,body,iframe{margin:0;width:100%;height:100%;border:0;display:block;overflow:hidden}body{background:#11101a}</style></head><body><iframe title="IEditor prototype" id="prototype"></iframe><script>
const pages=${json};const frame=document.getElementById('prototype');
function show(){const route=location.hash.slice(1);frame.srcdoc=pages[route]??pages.photo;}
window.addEventListener('hashchange',show);window.addEventListener('message',event=>{if(event.source===frame.contentWindow&&event.data?.kind==='ieditor-route'&&pages[event.data.route])location.hash=event.data.route;});show();
</script></body></html>\n`;
if(check){if(!fs.existsSync(output)||fs.readFileSync(output,'utf8').replace(/\r\n/g,'\n')!==html)throw Error('Prototype HTML is stale. Run node scripts/prototype/build.cjs.');console.log('PASS self-contained prototype is current.');}
else{fs.writeFileSync(output,html);console.log('Generated design/index.html ('+Buffer.byteLength(html)+' bytes).');}
