import { css as n, customElement as c, html as d } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as p } from "@umbraco-cms/backoffice/lit-element";
var i = Object.getOwnPropertyDescriptor, h = (r, s, l, a) => {
  for (var e = a > 1 ? void 0 : a ? i(s, l) : s, o = r.length - 1, m; o >= 0; o--)
    (m = r[o]) && (e = m(e) || e);
  return e;
};
let t = class extends p {
  render() {
    return d`
      <h1>Welcome Dashboard</h1>
      <div>
        <p>
          This is the Backoffice. From here, you can modify the content,
          media, and settings of your website.
        </p>
        <p>© Sample Company 20XX</p>
      </div>
    `;
  }
};
t.styles = [
  n`
      :host {
        display: block;
        padding: 24px;
      }
    `
];
t = h([
  c("my-welcome-dashboard")
], t);
const b = t;
export {
  t as MyWelcomeDashboardElement,
  b as default
};
//# sourceMappingURL=welcome-dashboard.js.map
