import { UMB_DOCUMENT_COLLECTION_CONTEXT as T, UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN as P } from "@umbraco-cms/backoffice/document";
import { css as D, state as d, customElement as x, html as n } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as E } from "@umbraco-cms/backoffice/lit-element";
import { UmbTextStyles as A } from "@umbraco-cms/backoffice/style";
import { fromCamelCase as O } from "@umbraco-cms/backoffice/utils";
var S = Object.defineProperty, $ = Object.getOwnPropertyDescriptor, y = (e) => {
  throw TypeError(e);
}, c = (e, t, a, s) => {
  for (var r = s > 1 ? void 0 : s ? $(t, a) : t, p = e.length - 1, m; p >= 0; p--)
    (m = e[p]) && (r = (s ? m(t, a, r) : m(r)) || r);
  return s && r && S(t, a, r), r;
}, f = (e, t, a) => t.has(e) || y("Cannot " + a), u = (e, t, a) => (f(e, t, "read from private field"), t.get(e)), _ = (e, t, a) => t.has(e) ? y("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, a), I = (e, t, a, s) => (f(e, t, "write to private field"), t.set(e, a), a), h = (e, t, a) => (f(e, t, "access private method"), a), b, o, l, w, g, v, C;
let i = class extends E {
  constructor() {
    super(), _(this, l), this._tableColumns = [], _(this, b, [
      {
        name: this.localize.term("general_name"),
        alias: "name",
        elementName: "umb-document-table-column-name",
        allowSorting: !0
      },
      {
        name: this.localize.term("content_publishStatus"),
        alias: "state",
        elementName: "umb-document-table-column-state",
        allowSorting: !1
      }
    ]), this._tableItems = [], _(this, o), this.consumeContext(T, (e) => {
      I(this, o, e), e?.setupView(this), this.observe(
        e?.workspacePathBuilder,
        (t) => {
          this._workspacePathBuilder = t, u(this, o) && h(this, l, v).call(this, u(this, o).getItems());
        },
        "observePath"
      ), h(this, l, w).call(this);
    });
  }
  render() {
    return n`
		<table class="doc-table">
				<thead>
					<tr>
						${this._tableColumns.map(
      (e) => n`<th style="text-align:${e.align ?? "left"}">${e.name}</th>`
    )}
					</tr>
				</thead>
				<tbody>
					${this._tableItems.map(
      (e) => n`
							<tr>
								${this._tableColumns.map((t) => {
        const s = e.data.find((r) => r.columnAlias === t.alias)?.value ?? "";
        return t.alias === "name" && s?.item ? n`<td><a href=${s.editPath || "#"}>${s.item.name}</a></td>` : t.alias === "state" && s?.item ? n`<td>${s.item.state}</td>` : t.alias === "entityActions" ? n`<td style="text-align:right;">⋮</td>` : n`<td>${s}</td>`;
      })}
							</tr>
						`
    )}
				</tbody>
			</table>`;
  }
};
b = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakMap();
l = /* @__PURE__ */ new WeakSet();
w = function() {
  u(this, o) && (this.observe(
    u(this, o).userDefinedProperties,
    (e) => {
      this._userDefinedProperties = e, h(this, l, g).call(this);
    },
    "_observeUserDefinedProperties"
  ), this.observe(
    u(this, o).items,
    (e) => {
      this._items = e, h(this, l, v).call(this, this._items);
    },
    "_observeItems"
  ));
};
g = function() {
  if (this._userDefinedProperties && this._userDefinedProperties.length > 0) {
    const e = this._userDefinedProperties.map((t) => ({
      name: this.localize.string(t.header),
      alias: t.alias,
      elementName: t.elementName,
      labelTemplate: t.nameTemplate,
      allowSorting: !0
    }));
    this._tableColumns = [
      ...u(this, b),
      ...e,
      { name: "", alias: "entityActions", align: "right" }
    ];
  }
};
v = function(e) {
  this._tableItems = e.map((t) => {
    if (!t.unique) throw new Error("Item id is missing.");
    const a = this._tableColumns?.map((s) => {
      if (s.alias === "entityActions")
        return {
          columnAlias: "entityActions",
          value: n`<umb-document-entity-actions-table-column-view
								.value=${t}></umb-document-entity-actions-table-column-view>`
        };
      const r = t.unique && this._workspacePathBuilder ? this._workspacePathBuilder({ entityType: t.entityType }) + P.generateLocal({
        unique: t.unique
      }) : "";
      return {
        columnAlias: s.alias,
        value: s.elementName ? { item: t, editPath: r } : h(this, l, C).call(this, t, s.alias)
      };
    }) ?? [];
    return {
      id: t.unique,
      icon: t.documentType.icon,
      entityType: "document",
      data: a
    };
  });
};
C = function(e, t) {
  switch (t) {
    case "contentTypeAlias":
      return e.contentTypeAlias;
    case "createDate":
      return e.createDate.toLocaleString();
    case "creator":
    case "owner":
      return e.creator;
    case "name":
      return e.name;
    case "state":
      return O(e.state);
    case "published":
      return e.state !== "Draft" ? "True" : "False";
    case "sortOrder":
      return e.sortOrder;
    case "updateDate":
      return e.updateDate.toLocaleString();
    case "updater":
      return e.updater;
    default:
      return e.values.find((a) => a.alias === t)?.value ?? "";
  }
};
i.styles = [
  A,
  D`
			:host {
				display: block;
				box-sizing: border-box;
				height: auto;
				width: 100%;
				padding: var(--uui-size-space-3) 0;
			}

			.container {
				display: flex;
				justify-content: center;
				align-items: center;
			}

			:host {
			display: block;
			width: 100%;
			overflow-x: auto;
			}
			table {
				width: 100%;
				border-collapse: collapse;
				font-size: 14px;
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
			`
];
c([
  d()
], i.prototype, "_workspacePathBuilder", 2);
c([
  d()
], i.prototype, "_userDefinedProperties", 2);
c([
  d()
], i.prototype, "_items", 2);
c([
  d()
], i.prototype, "_tableColumns", 2);
c([
  d()
], i.prototype, "_tableItems", 2);
i = c([
  x("my-document-table-collection-view")
], i);
const z = i;
export {
  i as MyDocumentTableCollectionViewElement,
  z as default
};
//# sourceMappingURL=my-collection-view.js.map
