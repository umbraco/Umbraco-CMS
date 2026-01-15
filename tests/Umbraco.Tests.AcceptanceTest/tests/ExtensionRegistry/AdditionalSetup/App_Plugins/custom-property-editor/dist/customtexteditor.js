import { css as p, property as u, state as c, customElement as g, html as l } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as m } from "@umbraco-cms/backoffice/lit-element";
import { UmbTextStyles as v } from "@umbraco-cms/backoffice/style";
var x = Object.defineProperty, _ = Object.getOwnPropertyDescriptor, o = (t, e, a, n) => {
  for (var r = n > 1 ? void 0 : n ? _(e, a) : e, i = t.length - 1, h; i >= 0; i--)
    (h = t[i]) && (r = (n ? h(e, a, r) : h(r)) || r);
  return n && r && x(e, a, r), r;
};
let s = class extends m {
  constructor() {
    super(...arguments), this.value = "";
  }
  render() {
    const t = this.value?.length || 0, a = this._maxLength !== null && this._maxLength !== void 0 && this._maxLength > 0;
    let n = "";
    return this._maxLength && (t > this._maxLength * 0.9 ? n = "danger" : t > this._maxLength * 0.7 && (n = "warning")), l`
              <uui-input
                  class="text-input"
                  type="text"
                  .value=${this.value || ""}
                  .placeholder=${this._placeholder || ""}
                  .maxlength=${this._maxLength || ""}
                  @input=${this._onInput}
                /></uui-input>
                ${a ? l`
                    <div class="char-counter ${n}">
                      ${t}/${this._maxLength}
                    </div>
                  ` : ""}
        `;
  }
  connectedCallback() {
    super.connectedCallback(), this._updateConfigValues();
  }
  _updateConfigValues() {
    this._maxLength = this.config?.getValueByAlias("maxChars"), this._placeholder = this.config?.getValueByAlias("placeholder") || "Enter text here...";
  }
  _onInput(t) {
    const e = t.target, a = e.value;
    if (this._maxLength && a.length > this._maxLength) {
      e.value = this.value;
      return;
    }
    this.value = a, this._dispatchChangeEvent();
  }
  _dispatchChangeEvent() {
    this.dispatchEvent(
      new CustomEvent("property-value-change", {
        detail: {
          value: this.value
        },
        bubbles: !0,
        composed: !0
      })
    );
  }
};
s.styles = [
  v,
  p`
            .text-input{
                width: 100%;
            }
            .char-counter {
              position: absolute;
              bottom: -20px;
              right: 0;
              font-size: 12px;
              color: var(--uui-color-text-alt);
            }

            .char-counter.warning {
              color: var(--uui-color-warning);
            }

            .char-counter.danger {
              color: var(--uui-color-danger);
            }
        `
];
o([
  u({ type: String })
], s.prototype, "value", 2);
o([
  u({ attribute: !1 })
], s.prototype, "config", 2);
o([
  c()
], s.prototype, "_maxLength", 2);
o([
  c()
], s.prototype, "_placeholder", 2);
s = o([
  g("custom-text-editor")
], s);
export {
  s as CustomTextEditorElement,
  s as default
};
//# sourceMappingURL=customtexteditor.js.map
