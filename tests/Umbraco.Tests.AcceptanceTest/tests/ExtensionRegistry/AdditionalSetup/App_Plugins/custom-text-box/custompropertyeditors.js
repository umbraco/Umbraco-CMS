import { css as e, customElement as t, html as n, property as r, state as i } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as a } from "@umbraco-cms/backoffice/lit-element";
import { UmbTextStyles as o } from "@umbraco-cms/backoffice/style";
//#region \0@oxc-project+runtime@0.127.0/helpers/decorate.js
function s(e, t, n, r) {
	var i = arguments.length, a = i < 3 ? t : r === null ? r = Object.getOwnPropertyDescriptor(t, n) : r, o;
	if (typeof Reflect == "object" && typeof Reflect.decorate == "function") a = Reflect.decorate(e, t, n, r);
	else for (var s = e.length - 1; s >= 0; s--) (o = e[s]) && (a = (i < 3 ? o(a) : i > 3 ? o(t, n, a) : o(t, n)) || a);
	return i > 3 && a && Object.defineProperty(t, n, a), a;
}
//#endregion
//#region src/custom-text-box.ts
var c = class extends a {
	constructor(...e) {
		super(...e), this.value = "", this._maxLength = null, this._placeholder = null;
	}
	connectedCallback() {
		super.connectedCallback(), this._updateConfigValues();
	}
	render() {
		let e = this.value?.length || 0, t = this._maxLength && this._maxLength > 0, r = "";
		return this._maxLength && (e > this._maxLength * .9 ? r = "danger" : e > this._maxLength * .7 && (r = "warning")), n`
      <uui-input
        class="text-input"
        type="text"
        .value=${this.value || ""}
        .placeholder=${this._placeholder || ""}
        .maxlength=${this._maxLength || ""}
        @input=${this._onInput}
      ></uui-input>

      ${t ? n`
            <div class="char-counter ${r}">
              ${e}/${this._maxLength}
            </div>
          ` : ""}
    `;
	}
	_updateConfigValues() {
		this._maxLength = this.config?.getValueByAlias("maxChars"), this._placeholder = this.config?.getValueByAlias("placeholder") || "Enter text here...";
	}
	_onInput(e) {
		let t = e.target, n = t.value;
		if (this._maxLength && n.length > this._maxLength) {
			t.value = this.value;
			return;
		}
		this.value = n, this._dispatchChangeEvent();
	}
	_dispatchChangeEvent() {
		this.dispatchEvent(new CustomEvent("change", {
			detail: { value: this.value },
			bubbles: !0,
			composed: !0
		}));
	}
	static {
		this.styles = [o, e`
      .text-input {
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
    `];
	}
};
s([r({ type: String })], c.prototype, "value", void 0), s([r({ attribute: !1 })], c.prototype, "config", void 0), s([i()], c.prototype, "_maxLength", void 0), s([i()], c.prototype, "_placeholder", void 0), c = s([t("custom-text-editor")], c);
var l = c;
//#endregion
export { l as default };

//# sourceMappingURL=custompropertyeditors.js.map