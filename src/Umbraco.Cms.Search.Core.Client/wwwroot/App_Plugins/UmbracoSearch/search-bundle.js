import { UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS as e, UMB_SEARCH_COLLECTION_VIEW_ALIAS as t, UMB_SEARCH_DETAIL_REPOSITORY_ALIAS as n, UMB_SEARCH_DETAIL_STORE_ALIAS as r, UMB_SEARCH_INDEX_ENTITY_TYPE as i, UMB_SEARCH_QUERY_REPOSITORY_ALIAS as a, UMB_SEARCH_ROOT_COLLECTION_ALIAS as o, UMB_SEARCH_ROOT_ENTITY_TYPE as s, UMB_SEARCH_ROOT_WORKSPACE_ALIAS as c, UMB_SEARCH_WORKSPACE_ALIAS as l, UmbSearchContext as u } from "@umbraco-cms/search/global";
import { UMB_WORKSPACE_CONDITION_ALIAS as d } from "@umbraco-cms/backoffice/workspace";
import { UMB_ADVANCED_SETTINGS_MENU_ALIAS as f } from "@umbraco-cms/backoffice/settings";
//#endregion
//#region src/bundle/lang/manifests.ts
var p = [{
	type: "localization",
	name: "Umbraco Search Localization - English",
	alias: "Umbraco.Search.Localization.En",
	meta: {
		culture: "en",
		localizations: { search: {
			treeHeader: "Search",
			tableColumnAlias: "Alias",
			tableColumnHealthStatus: "Health Status",
			tableColumnDocumentCount: "Document Count",
			healthStatus: (e) => e,
			documentCount: (e) => {
				switch (e) {
					case 0: return "Empty";
					case 1: return "1 document";
					default: return `${e} documents`;
				}
			},
			collectionActionReload: "Refresh list",
			entityActionRebuildIndex: "Rebuild Index",
			rebuildConfirmHeadline: "Rebuild Search Index",
			rebuildConfirmMessage: "Are you sure you want to rebuild the search index? This operation may take a while depending on the size of your content.",
			rebuildConfirmLabel: "Rebuild Index",
			rebuildStartedMessage: "The rebuild of search index \"{0}\" has started. You can continue working while the process runs in the background.",
			rebuildCompletedTitle: "Search Index Rebuild Completed",
			rebuildCompletedMessage: "The rebuild of search index \"{0}\" has completed successfully.",
			rebuildingIndex: "Rebuilding index...",
			rebuildIndex: "Rebuild Index",
			indexInfo: "Index Information",
			indexAlias: "Index Alias",
			providerName: "Provider Name",
			searchBox: "Search",
			searchPlaceholder: "Search index...",
			searchButton: "Search",
			noResults: "No results found",
			resultsCount: (e) => `Found ${e} result${e === 1 ? "" : "s"}`,
			tableColumnName: "Name",
			tableColumnEntityType: "Entity Type",
			statsBoxLabel: "Statistics",
			searchBoxLabel: "Search",
			searching: "Searching...",
			searchFailed: "Search failed",
			searchComplete: (e) => `Search complete. Found ${e} result${e === 1 ? "" : "s"}`,
			openEntity: (e, t) => `Open ${e} with ID ${t}`,
			searchFormLabel: (e) => `Search ${e} index`,
			searchInputLabel: "Search query",
			searchInputAriaLabel: (e) => `Enter search query for ${e} index`,
			searchButtonAriaLabel: "Execute search",
			searchHint: "Press Enter or click Search button to execute search",
			loading: "Loading search results",
			resultsRegion: "Search results",
			resultsTable: "Search results table",
			paginationLabel: "Search results pages",
			cultureSelectLabel: "Culture",
			searchDisabled: "Search is disabled because the index is not healthy. Current status:",
			searchError: "An error occurred while searching. Please try again."
		} }
	}
}, {
	type: "localization",
	name: "Umbraco Search Localization - Danish",
	alias: "Umbraco.Search.Localization.Da",
	meta: { culture: "da" },
	js: () => import("./da-BNWAqFww.js")
}], m = [{
	type: "collectionAction",
	kind: "button",
	name: "Umbraco Search Collection Action - Reload",
	alias: "Umbraco.Search.CollectionAction.Reload",
	api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchCollectionReloadAction })),
	meta: { label: "#search_collectionActionReload" },
	conditions: [{
		alias: "Umb.Condition.CollectionAlias",
		match: o
	}]
}], h = [{
	type: "collection",
	kind: "default",
	name: "Umbraco Search - Root Collection",
	alias: o,
	api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchCollectionContext })),
	meta: { repositoryAlias: e }
}, {
	type: "collectionView",
	name: "Umbraco Search - Root Collection View",
	alias: t,
	element: "@umbraco-cms/search/settings",
	elementName: "umb-search-root-collection-view",
	meta: {
		label: "#search_treeHeader",
		icon: "icon-search",
		pathName: "table"
	},
	conditions: [{
		alias: "Umb.Condition.CollectionAlias",
		match: o
	}]
}], g = [{
	type: "condition",
	name: "Search Index Provider Name Condition",
	alias: "Umb.Search.Condition.IndexProviderName",
	api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchIndexProviderNameCondition }))
}], _ = [{
	type: "entityAction",
	kind: "default",
	alias: "Umb.Search.EntityAction.RebuildIndex",
	name: "Umbraco Search Entity Action - Rebuild Index",
	weight: 100,
	api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchRebuildIndexEntityAction })),
	forEntityTypes: [i],
	meta: {
		icon: "icon-refresh",
		label: "#search_entityActionRebuildIndex",
		additionalOptions: !1
	}
}], v = [{
	type: "globalContext",
	alias: "Umbraco.Search.GlobalContext",
	name: "Umbraco Search Global Context",
	api: u
}], y = [
	{
		type: "repository",
		name: "Umbraco Search Collection Repository",
		alias: e,
		api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchCollectionRepository }))
	},
	{
		type: "repository",
		name: "Umbraco Search Detail Repository",
		alias: n,
		api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchDetailRepository }))
	},
	{
		type: "repository",
		name: "Umbraco Search Query Repository",
		alias: a,
		api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchQueryRepository }))
	},
	{
		type: "store",
		name: "Umbraco Search Detail Store",
		alias: r,
		api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchDetailStore }))
	}
], b = [{
	type: "workspace",
	kind: "default",
	name: "Umbraco Search - Workspace",
	alias: c,
	meta: {
		entityType: s,
		headline: "#search_treeHeader"
	}
}, {
	type: "workspaceView",
	kind: "collection",
	name: "Umbraco Search - Workspace View",
	alias: "Umbraco.Search.WorkspaceView.Collection",
	meta: {
		label: "#search_treeHeader",
		pathname: "indexes",
		icon: "icon-search",
		collectionAlias: o
	},
	conditions: [{
		alias: "Umb.Condition.WorkspaceAlias",
		match: c
	}]
}], x = [{
	type: "workspace",
	kind: "routable",
	alias: l,
	name: "Search Workspace",
	api: () => import("@umbraco-cms/search/settings").then((e) => ({ default: e.UmbSearchWorkspaceContext })),
	meta: { entityType: i }
}, {
	type: "workspaceView",
	alias: "Umb.WorkspaceView.Search.Details",
	name: "Search Details View",
	element: "@umbraco-cms/search/settings",
	elementName: "umb-search-details-view",
	weight: 300,
	meta: {
		label: "#general_details",
		pathname: "details",
		icon: "icon-search"
	},
	conditions: [{
		alias: d,
		match: l
	}]
}], S = [{
	type: "searchIndexDetailBox",
	alias: "Umb.SearchIndexDetailBox.Stats",
	name: "Search Index Stats Box",
	weight: 100,
	element: "@umbraco-cms/search/settings",
	elementName: "umb-search-index-stats-box",
	meta: {
		label: "#search_statsBoxLabel",
		column: "right"
	}
}, {
	type: "searchIndexDetailBox",
	alias: "Umb.SearchIndexDetailBox.Search",
	name: "Search Index Search Box",
	weight: 100,
	element: "@umbraco-cms/search/settings",
	elementName: "umb-search-index-search-box",
	meta: {
		label: "#search_searchBoxLabel",
		column: "left"
	}
}], C = {
	type: "kind",
	alias: "Umb.Kind.SearchIndexDetailBox",
	matchKind: "default",
	matchType: "searchIndexDetailBox",
	manifest: {
		type: "searchIndexDetailBox",
		kind: "default"
	}
}, w = [
	...v,
	...p,
	...y,
	...m,
	...h,
	...g,
	..._,
	...b,
	...x,
	...S,
	C,
	{
		type: "menuItem",
		name: "Umbraco Search Root Menu Item",
		alias: "Umbraco.Search.Root.MenuItem",
		meta: {
			label: "#search_treeHeader",
			entityType: s,
			icon: "icon-search",
			menus: [f]
		}
	}
];
//#endregion
export { w as manifests };

//# sourceMappingURL=search-bundle.js.map