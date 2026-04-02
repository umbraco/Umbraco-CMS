import { LitElement as n, html as a, css as c, customElement as p } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as u } from "@umbraco-cms/backoffice/element-api";
var w = Object.getOwnPropertyDescriptor, v = (o, s, i, l) => {
  for (var e = l > 1 ? void 0 : l ? w(s, i) : s, r = o.length - 1, m; r >= 0; r--)
    (m = o[r]) && (e = m(e) || e);
  return e;
};
let t = class extends u(n) {
  render() {
    return a`     
     <uui-box headline="Workspace View">
        Welcome to my newly created workspace view.
      </uui-box>            
    `;
  }
};
t.styles = c`
    uui-box {
      margin: 20px;
    }
  `;
t = v([
  p("my-workspaceview")
], t);
export {
  t as default
};
//# sourceMappingURL=workspace-view.js.map
