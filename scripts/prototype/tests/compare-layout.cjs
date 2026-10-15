// 对照共同外壳和预览舞台的真实坐标；不把坐标通过当作整页像素相似率。
const fs=require('fs'),path=require('path');
const [nativeDir,prototypeDir]=process.argv.slice(2);
if(!nativeDir||!prototypeDir)throw Error('Usage: node compare-layout.cjs <native-evidence> <prototype-evidence>');
function read(file){return new Map(fs.readFileSync(file,'utf8').trim().split(/\r?\n/).slice(1).map(line=>{const [key,...values]=line.split(',');return [key,values.map(Number)];}));}
let count=0;const failures=[];
for(const [nativeFile,prototypeFile,keys]of [
 ['photo-empty-en-US','photo-en-US-Light',['ie-topbar-0','ie-statusbar-0','ie-stage-0']],
 ['batch-loaded-en-US','batch-en-US-Light',['ie-topbar-0','ie-statusbar-0']],
 ['settings-en-US','settings-en-US-Light',['ie-topbar-0','ie-statusbar-0']]]){
 const native=read(path.join(nativeDir,nativeFile+'.geometry.csv')),prototype=read(path.join(prototypeDir,prototypeFile+'.geometry.csv'));
 for(const key of keys){const actual=native.get(key),expected=prototype.get(key);if(!actual||!expected)throw Error('Missing '+key);
  actual.forEach((value,index)=>{count++;const delta=Math.abs(value-expected[index]);if(delta>2)failures.push(nativeFile+' '+key+' '+['x','y','width','height'][index]+': '+value+' vs '+expected[index]);});
 }
}
if(failures.length)throw Error(failures.join('\n'));
console.log('PASS '+count+' measured shell/stage coordinates within 2 DIP at 1320 x 860. This is not a whole-page pixel similarity score.');
