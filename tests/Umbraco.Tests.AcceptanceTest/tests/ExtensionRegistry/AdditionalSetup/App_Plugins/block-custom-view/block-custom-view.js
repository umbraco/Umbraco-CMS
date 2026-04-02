import { LitElement as c, html as m, css as h, property as a, customElement as u } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as b } from "@umbraco-cms/backoffice/element-api";
var d = Object.defineProperty, f = Object.getOwnPropertyDescriptor, l = (p, o, s, r) => {
  for (var e = r > 1 ? void 0 : r ? f(o, s) : o, i = p.length - 1, n; i >= 0; i--)
    (n = p[i]) && (e = (r ? n(o, s, e) : n(e)) || e);
  return r && e && d(o, s, e), e;
};
let t = class extends b(c) {
  render() {
    return m`
			<h5>My Custom View</h5>
			<p>Heading and Theme: ${this.content?.heading} - ${this.settings?.theme}</p>
		`;
  }
};
t.styles = [
  h`
			:host {
				display: block;
				height: 100%;
				box-sizing: border-box;
				background-color: darkgreen;
				color: white;
				border-radius: 9px;
				padding: 12px;
			}
		`
];
l([
  a({ attribute: !1 })
], t.prototype, "content", 2);
l([
  a({ attribute: !1 })
], t.prototype, "settings", 2);
t = l([
  u("block-custom-view")
], t);
const w = t;
export {
  t as BlockCustomView,
  w as default
};
//# sourceMappingURL=block-custom-view.js.map
