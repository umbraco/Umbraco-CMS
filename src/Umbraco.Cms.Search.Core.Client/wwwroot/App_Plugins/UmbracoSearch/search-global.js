import { UmbContextBase as e } from "@umbraco-cms/backoffice/class-api";
import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT as t } from "@umbraco-cms/backoffice/management-api";
import { UmbLocalizationController as n } from "@umbraco-cms/backoffice/localization-api";
import { UMB_NOTIFICATION_CONTEXT as r } from "@umbraco-cms/backoffice/notification";
import { UmbContextToken as i } from "@umbraco-cms/backoffice/context-api";
import { UmbBasicState as a } from "@umbraco-cms/backoffice/observable-api";
//#region src/global/constants.ts
var o = "search-root", s = "search", c = "search-index", l = "search-document", u = "UMB_SEARCH_COLLECTION_REPOSITORY", d = "UmbSearchDetailRepository", f = "UmbSearchStore", p = "UmbSearchQueryRepository", m = "UMB_SEARCH_ROOT_COLLECTION", h = "Umbraco.Search.Workspace.Root", g = "Umbraco.Search.Workspace", _ = "Umbraco.Search.CollectionView.Root", v = "IndexRebuildCompleted", y = class extends e {
	#e;
	#t;
	#n;
	#r;
	#i;
	constructor(e) {
		super(e, b), this.#n = /* @__PURE__ */ new Set(), this.#r = new n(this), this.#i = new a(void 0), this.indexRebuilt = this.#i.asObservable(), this.consumeContext(r, (e) => {
			this.#t = e;
		}), this.consumeContext(t, (e) => {
			this.#e = e, this.#o();
		});
	}
	setUserWaitingForIndexUpdate(e, t) {
		t ? this.#n.add(e) : this.#n.delete(e);
	}
	#a(e) {
		return this.#n.has(e);
	}
	#o() {
		this.observe(this.#e?.byEventSource(v), (e) => {
			if (!e?.eventSource) return;
			let t = String(e.eventSource);
			this.#i.setValue(t), this.#a(t) && (this.setUserWaitingForIndexUpdate(t, !1), this.#t?.peek("positive", { data: {
				title: this.#r.term("search_rebuildCompletedTitle"),
				message: this.#r.term("search_rebuildCompletedMessage", t)
			} }));
		}, "index-rebuild-notification-observer");
	}
}, b = new i("UmbSearchContext");
//#endregion
export { u as UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS, _ as UMB_SEARCH_COLLECTION_VIEW_ALIAS, b as UMB_SEARCH_CONTEXT, d as UMB_SEARCH_DETAIL_REPOSITORY_ALIAS, f as UMB_SEARCH_DETAIL_STORE_ALIAS, l as UMB_SEARCH_DOCUMENT_ENTITY_TYPE, s as UMB_SEARCH_ENTITY_TYPE, c as UMB_SEARCH_INDEX_ENTITY_TYPE, p as UMB_SEARCH_QUERY_REPOSITORY_ALIAS, m as UMB_SEARCH_ROOT_COLLECTION_ALIAS, o as UMB_SEARCH_ROOT_ENTITY_TYPE, h as UMB_SEARCH_ROOT_WORKSPACE_ALIAS, v as UMB_SEARCH_SERVER_EVENT_TYPE, g as UMB_SEARCH_WORKSPACE_ALIAS, y as UmbSearchContext };

//# sourceMappingURL=search-global.js.map