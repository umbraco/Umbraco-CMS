import { LitElement as n, html as c, customElement as u } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as a } from "@umbraco-cms/backoffice/element-api";
var d = Object.getOwnPropertyDescriptor, p = (o, l, i, m) => {
  for (var e = m > 1 ? void 0 : m ? d(l, i) : l, t = o.length - 1, s; t >= 0; t--)
    (s = o[t]) && (e = s(e) || e);
  return e;
};
let r = class extends a(n) {
  render() {
    return c`
            <h5> Block Grid Custom View</h5>
		`;
  }
};
r = p([
  u("block-grid-custom-view")
], r);
const f = r;
export {
  r as BlockGridCustomView,
  f as default
};
//# sourceMappingURL=block-grid-custom-view.js.map
