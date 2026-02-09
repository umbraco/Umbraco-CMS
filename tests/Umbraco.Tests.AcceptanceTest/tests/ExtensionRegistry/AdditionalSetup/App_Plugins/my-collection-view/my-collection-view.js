import { css as u, state as d, customElement as _, html as n } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as f } from "@umbraco-cms/backoffice/lit-element";
import { UMB_DOCUMENT_COLLECTION_CONTEXT as v } from "@umbraco-cms/backoffice/document";
var y = Object.defineProperty, w = Object.getOwnPropertyDescriptor, h = (t) => {
  throw TypeError(t);
}, m = (t, e, a, s) => {
  for (var i = s > 1 ? void 0 : s ? w(e, a) : e, o = t.length - 1, l; o >= 0; o--)
    (l = t[o]) && (i = (s ? l(e, a, i) : l(i)) || i);
  return s && i && y(e, a, i), i;
}, b = (t, e, a) => e.has(t) || h("Cannot " + a), C = (t, e, a) => e.has(t) ? h("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, a), E = (t, e, a) => (b(t, e, "access private method"), a), c, p;
let r = class extends f {
  constructor() {
    super(), C(this, c), this._columns = [], this._items = [], this.consumeContext(v, (t) => {
      t?.setupView(this), this.observe(t?.userDefinedProperties, (e) => {
        E(this, c, p).call(this, e);
      }), this.observe(t?.items, (e) => {
        this._items = e;
      });
    });
  }
  render() {
    return this._items === void 0 ? n`<p>Not found...</p>` : n`
			<table>
				<thead>
					<tr>
						${this._columns.map((t) => n`<th style="text-align:${t.align ?? "left"}">${t.name}</th>`)}
					</tr>
				</thead>
				<tbody>
					${this._items.map(
      (t) => n`
							<tr>
								${this._columns.map((e) => {
        switch (e.alias) {
          case "name":
            return n`<td><a href="#">${t.variants[0].name}</a></td>`;
          case "entityActions":
            return n`<td style="text-align:right;">⋮</td>`;
          default:
            const a = t.values.find((s) => s.alias === e.alias)?.value ?? "";
            return n`<td>${a}</td>`;
        }
      })}
							</tr>
						`
    )}
				</tbody>
			</table>
		`;
  }
};
c = /* @__PURE__ */ new WeakSet();
p = function(t = []) {
  const e = [
    { name: "Name", alias: "name" },
    { name: "State", alias: "state" }
  ], a = t.map((s) => ({
    name: s.nameTemplate ?? s.alias,
    alias: s.alias
  }));
  this._columns = [...e, ...a, { name: "", alias: "entityActions", align: "right" }];
};
r.styles = u`
		:host {
			display: block;
			width: 100%;
			overflow-x: auto;
			font-family: sans-serif;
		}
		table {
			width: 100%;
			border-collapse: collapse;
		}
		th,
		td {
			padding: 6px 10px;
			border: 1px solid #ddd;
			white-space: nowrap;
		}
		th {
			background: #f8f8f8;
			font-weight: 600;
		}
		a {
			color: var(--uui-color-interactive, #0366d6);
			text-decoration: none;
		}
		a:hover {
			text-decoration: underline;
		}
	`;
m([
  d()
], r.prototype, "_columns", 2);
m([
  d()
], r.prototype, "_items", 2);
r = m([
  _("my-document-table-collection-view")
], r);
const O = r;
export {
  r as MyDocumentTableCollectionViewElement,
  O as default
};
//# sourceMappingURL=my-collection-view.js.map
