/* IEditor 原型 · 通用交互（分段选择 / 色板 / 规格卡 / 滑杆着色） */
(function () {
  function activateIn(container, selector, cls) {
    container.querySelectorAll(selector).forEach(function (el) {
      el.classList.toggle(cls, el === container.__active);
    });
  }

  document.addEventListener('click', function (e) {
    // 分段选择
    var segOpt = e.target.closest('.seg [data-value]');
    if (segOpt && !segOpt.disabled) {
      var seg = segOpt.closest('.seg');
      seg.querySelectorAll('[data-value]').forEach(function (b) {
        b.classList.toggle('on', b === segOpt);
      });
      seg.dispatchEvent(new CustomEvent('segchange', { bubbles: true, detail: { value: segOpt.dataset.value } }));
    }

    // 色板
    var sw = e.target.closest('.swatch');
    if (sw) {
      var group = sw.closest('.swatches');
      group.querySelectorAll('.swatch').forEach(function (s) { s.classList.toggle('on', s === sw); });
      group.dispatchEvent(new CustomEvent('swchange', { bubbles: true, detail: { color: sw.dataset.color || null } }));
    }

    // 规格卡片
    var spec = e.target.closest('.spec');
    if (spec && !spec.disabled) {
      var grid = spec.closest('.spec-grid');
      grid.querySelectorAll('.spec').forEach(function (s) { s.classList.toggle('on', s === spec); });
      document.dispatchEvent(new CustomEvent('specchange', { detail: { id: spec.dataset.spec } }));
    }
  });

  // 滑杆轨道着色
  function paintRange(r) {
    var min = Number(r.min) || 0, max = Number(r.max) || 100;
    var p = ((Number(r.value) - min) / (max - min)) * 100;
    r.style.setProperty('--p', p + '%');
  }
  document.querySelectorAll('input[type="range"]').forEach(function (r) {
    paintRange(r);
    r.addEventListener('input', function () { paintRange(r); });
  });
})();
