import { UMB_SEARCH_COLLECTION_VIEW_ALIAS as e, UMB_SEARCH_CONTEXT as t, UMB_SEARCH_DETAIL_REPOSITORY_ALIAS as n, UMB_SEARCH_DOCUMENT_ENTITY_TYPE as r, UMB_SEARCH_INDEX_ENTITY_TYPE as i, UMB_SEARCH_ROOT_ENTITY_TYPE as a, UMB_SEARCH_WORKSPACE_ALIAS as o } from "@umbraco-cms/search/global";
import { UMB_WORKSPACE_MODAL as s, UMB_WORKSPACE_PATH_PATTERN as c, UmbEntityNamedDetailWorkspaceContextBase as l } from "@umbraco-cms/backoffice/workspace";
import { UMB_SETTINGS_SECTION_PATHNAME as u } from "@umbraco-cms/backoffice/settings";
import { UmbControllerBase as d } from "@umbraco-cms/backoffice/class-api";
import { UmbLocalizationController as f } from "@umbraco-cms/backoffice/localization-api";
import { UMB_NOTIFICATION_CONTEXT as p } from "@umbraco-cms/backoffice/notification";
import { UmbContextToken as m } from "@umbraco-cms/backoffice/context-api";
import { UmbStringState as h, observeMultiple as ee } from "@umbraco-cms/backoffice/observable-api";
import { client as g } from "@umbraco-cms/backoffice/external/backend-api";
import { UmbError as _, tryExecute as v } from "@umbraco-cms/backoffice/resources";
import { UmbDetailRepositoryBase as te, UmbRepositoryBase as y } from "@umbraco-cms/backoffice/repository";
import { UMB_COLLECTION_CONTEXT as b, UmbCollectionActionBase as ne, UmbDefaultCollectionContext as re } from "@umbraco-cms/backoffice/collection";
import { UmbEntityActionBase as ie } from "@umbraco-cms/backoffice/entity-action";
import { umbConfirmModal as ae } from "@umbraco-cms/backoffice/modal";
import { UmbLitElement as x } from "@umbraco-cms/backoffice/lit-element";
import { css as S, customElement as C, html as w, nothing as T, state as E, when as D } from "@umbraco-cms/backoffice/external/lit";
import { UmbTextStyles as O } from "@umbraco-cms/backoffice/style";
import { UmbConditionBase as oe } from "@umbraco-cms/backoffice/extension-registry";
import { UMB_SEARCH_DETAIL_STORE_CONTEXT as se, UMB_SEARCH_WORKSPACE_CONTEXT as ce, UmbSearchDetailRepository as le } from "@umbraco-cms/search/settings";
import { UmbPaginationManager as ue, debounce as de, stringOrStringArrayContains as fe } from "@umbraco-cms/backoffice/utils";
import { UmbDetailStoreBase as pe } from "@umbraco-cms/backoffice/store";
import { UmbModalRouteRegistrationController as me } from "@umbraco-cms/backoffice/router";
import { UMB_APP_LANGUAGE_CONTEXT as he } from "@umbraco-cms/backoffice/language";
//#region src/settings/api/core/bodySerializer.gen.ts
var ge = { bodySerializer: (e) => JSON.stringify(e, (e, t) => typeof t == "bigint" ? t.toString() : t) };
Object.entries({
	$body_: "body",
	$headers_: "headers",
	$path_: "path",
	$query_: "query"
});
//#endregion
//#region src/settings/api/core/serverSentEvents.gen.ts
function _e({ onRequest: e, onSseError: t, onSseEvent: n, responseTransformer: r, responseValidator: i, sseDefaultRetryDelay: a, sseMaxRetryAttempts: o, sseMaxRetryDelay: s, sseSleepFn: c, url: l, ...u }) {
	let d, f = c ?? ((e) => new Promise((t) => setTimeout(t, e)));
	return { stream: async function* () {
		let c = a ?? 3e3, p = 0, m = u.signal ?? new AbortController().signal;
		for (; !m.aborted;) {
			p++;
			let a = u.headers instanceof Headers ? u.headers : new Headers(u.headers);
			d !== void 0 && a.set("Last-Event-ID", d);
			try {
				let t = {
					redirect: "follow",
					...u,
					body: u.serializedBody,
					headers: a,
					signal: m
				}, o = new Request(l, t);
				e && (o = await e(l, t));
				let s = await (u.fetch ?? globalThis.fetch)(o);
				if (!s.ok) throw Error(`SSE failed: ${s.status} ${s.statusText}`);
				if (!s.body) throw Error("No body in SSE response");
				let f = s.body.pipeThrough(new TextDecoderStream()).getReader(), p = "", h = () => {
					try {
						f.cancel();
					} catch {}
				};
				m.addEventListener("abort", h);
				try {
					for (;;) {
						let { done: e, value: t } = await f.read();
						if (e) break;
						p += t, p = p.replace(/\r\n?/g, "\n");
						let a = p.split("\n\n");
						p = a.pop() ?? "";
						for (let e of a) {
							let t = e.split("\n"), a = [], o;
							for (let e of t) if (e.startsWith("data:")) a.push(e.replace(/^data:\s*/, ""));
							else if (e.startsWith("event:")) o = e.replace(/^event:\s*/, "");
							else if (e.startsWith("id:")) d = e.replace(/^id:\s*/, "");
							else if (e.startsWith("retry:")) {
								let t = Number.parseInt(e.replace(/^retry:\s*/, ""), 10);
								Number.isNaN(t) || (c = t);
							}
							let s, l = !1;
							if (a.length) {
								let e = a.join("\n");
								try {
									s = JSON.parse(e), l = !0;
								} catch {
									s = e;
								}
							}
							l && (i && await i(s), r && (s = await r(s))), n?.({
								data: s,
								event: o,
								id: d,
								retry: c
							}), a.length && (yield s);
						}
					}
				} finally {
					m.removeEventListener("abort", h), f.releaseLock();
				}
				break;
			} catch (e) {
				if (t?.(e), o !== void 0 && p >= o) break;
				let n = Math.min(c * 2 ** (p - 1), s ?? 3e4);
				await f(n);
			}
		}
	}() };
}
//#endregion
//#region src/settings/api/core/pathSerializer.gen.ts
var ve = (e) => {
	switch (e) {
		case "label": return ".";
		case "matrix": return ";";
		case "simple": return ",";
		default: return "&";
	}
}, ye = (e) => {
	switch (e) {
		case "form": return ",";
		case "pipeDelimited": return "|";
		case "spaceDelimited": return "%20";
		default: return ",";
	}
}, be = (e) => {
	switch (e) {
		case "label": return ".";
		case "matrix": return ";";
		case "simple": return ",";
		default: return "&";
	}
}, k = ({ allowReserved: e, explode: t, name: n, style: r, value: i }) => {
	if (!t) {
		let t = (e ? i : i.map((e) => encodeURIComponent(e))).join(ye(r));
		switch (r) {
			case "label": return `.${t}`;
			case "matrix": return `;${n}=${t}`;
			case "simple": return t;
			default: return `${n}=${t}`;
		}
	}
	let a = ve(r), o = i.map((t) => r === "label" || r === "simple" ? e ? t : encodeURIComponent(t) : A({
		allowReserved: e,
		name: n,
		value: t
	})).join(a);
	return r === "label" || r === "matrix" ? a + o : o;
}, A = ({ allowReserved: e, name: t, value: n }) => {
	if (n == null) return "";
	if (typeof n == "object") throw Error("Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these.");
	return `${t}=${e ? n : encodeURIComponent(n)}`;
}, j = ({ allowReserved: e, explode: t, name: n, style: r, value: i, valueOnly: a }) => {
	if (i instanceof Date) return a ? i.toISOString() : `${n}=${i.toISOString()}`;
	if (r !== "deepObject" && !t) {
		let t = [];
		Object.entries(i).forEach(([n, r]) => {
			t = [
				...t,
				n,
				e ? r : encodeURIComponent(r)
			];
		});
		let a = t.join(",");
		switch (r) {
			case "form": return `${n}=${a}`;
			case "label": return `.${a}`;
			case "matrix": return `;${n}=${a}`;
			default: return a;
		}
	}
	let o = be(r), s = Object.entries(i).map(([t, i]) => A({
		allowReserved: e,
		name: r === "deepObject" ? `${n}[${t}]` : t,
		value: i
	})).join(o);
	return r === "label" || r === "matrix" ? o + s : s;
}, xe = /\{[^{}]+\}/g, Se = ({ path: e, url: t }) => {
	let n = t, r = t.match(xe);
	if (r) for (let t of r) {
		let r = !1, i = t.substring(1, t.length - 1), a = "simple";
		i.endsWith("*") && (r = !0, i = i.substring(0, i.length - 1)), i.startsWith(".") ? (i = i.substring(1), a = "label") : i.startsWith(";") && (i = i.substring(1), a = "matrix");
		let o = e[i];
		if (o == null) continue;
		if (Array.isArray(o)) {
			n = n.replace(t, k({
				explode: r,
				name: i,
				style: a,
				value: o
			}));
			continue;
		}
		if (typeof o == "object") {
			n = n.replace(t, j({
				explode: r,
				name: i,
				style: a,
				value: o,
				valueOnly: !0
			}));
			continue;
		}
		if (a === "matrix") {
			n = n.replace(t, `;${A({
				name: i,
				value: o
			})}`);
			continue;
		}
		let s = encodeURIComponent(a === "label" ? `.${o}` : o);
		n = n.replace(t, s);
	}
	return n;
}, Ce = ({ baseUrl: e, path: t, query: n, querySerializer: r, url: i }) => {
	let a = i.startsWith("/") ? i : `/${i}`, o = (e ?? "") + a;
	t && (o = Se({
		path: t,
		url: o
	}));
	let s = n ? r(n) : "";
	return s.startsWith("?") && (s = s.substring(1)), s && (o += `?${s}`), o;
};
function M(e) {
	let t = e.body !== void 0;
	if (t && e.bodySerializer) return "serializedBody" in e ? e.serializedBody !== void 0 && e.serializedBody !== "" ? e.serializedBody : null : e.body === "" ? null : e.body;
	if (t) return e.body;
}
//#endregion
//#region src/settings/api/core/auth.gen.ts
var we = async (e, t) => {
	let n = typeof t == "function" ? await t(e) : t;
	if (n) return e.scheme === "bearer" ? `Bearer ${n}` : e.scheme === "basic" ? `Basic ${btoa(n)}` : n;
}, N = ({ parameters: e = {}, ...t } = {}) => (n) => {
	let r = [];
	if (n && typeof n == "object") for (let i in n) {
		let a = n[i];
		if (a == null) continue;
		let o = e[i] || t;
		if (Array.isArray(a)) {
			let e = k({
				allowReserved: o.allowReserved,
				explode: !0,
				name: i,
				style: "form",
				value: a,
				...o.array
			});
			e && r.push(e);
		} else if (typeof a == "object") {
			let e = j({
				allowReserved: o.allowReserved,
				explode: !0,
				name: i,
				style: "deepObject",
				value: a,
				...o.object
			});
			e && r.push(e);
		} else {
			let e = A({
				allowReserved: o.allowReserved,
				name: i,
				value: a
			});
			e && r.push(e);
		}
	}
	return r.join("&");
}, Te = (e) => {
	if (!e) return "stream";
	let t = e.split(";")[0]?.trim();
	if (t) {
		if (t.startsWith("application/json") || t.endsWith("+json")) return "json";
		if (t === "multipart/form-data") return "formData";
		if ([
			"application/",
			"audio/",
			"image/",
			"video/"
		].some((e) => t.startsWith(e))) return "blob";
		if (t.startsWith("text/")) return "text";
	}
}, Ee = (e, t) => t ? !!(e.headers.has(t) || e.query?.[t] || e.headers.get("Cookie")?.includes(`${t}=`)) : !1;
async function De(e) {
	for (let t of e.security ?? []) {
		if (Ee(e, t.name)) continue;
		let n = await we(t, e.auth);
		if (!n) continue;
		let r = t.name ?? "Authorization";
		switch (t.in) {
			case "query":
				e.query ||= {}, e.query[r] = n;
				break;
			case "cookie":
				e.headers.append("Cookie", `${r}=${n}`);
				break;
			default:
				e.headers.set(r, n);
				break;
		}
	}
}
var P = (e) => Ce({
	baseUrl: e.baseUrl,
	path: e.path,
	query: e.query,
	querySerializer: typeof e.querySerializer == "function" ? e.querySerializer : N(e.querySerializer),
	url: e.url
}), F = (e, t) => {
	let n = {
		...e,
		...t
	};
	return n.baseUrl?.endsWith("/") && (n.baseUrl = n.baseUrl.substring(0, n.baseUrl.length - 1)), n.headers = I(e.headers, t.headers), n;
}, Oe = (e) => {
	let t = [];
	return e.forEach((e, n) => {
		t.push([n, e]);
	}), t;
}, I = (...e) => {
	let t = new Headers();
	for (let n of e) {
		if (!n) continue;
		let e = n instanceof Headers ? Oe(n) : Object.entries(n);
		for (let [n, r] of e) if (r === null) t.delete(n);
		else if (Array.isArray(r)) for (let e of r) t.append(n, e);
		else r !== void 0 && t.set(n, typeof r == "object" ? JSON.stringify(r) : r);
	}
	return t;
}, L = class {
	constructor() {
		this.fns = [];
	}
	clear() {
		this.fns = [];
	}
	eject(e) {
		let t = this.getInterceptorIndex(e);
		this.fns[t] && (this.fns[t] = null);
	}
	exists(e) {
		let t = this.getInterceptorIndex(e);
		return !!this.fns[t];
	}
	getInterceptorIndex(e) {
		return typeof e == "number" ? this.fns[e] ? e : -1 : this.fns.indexOf(e);
	}
	update(e, t) {
		let n = this.getInterceptorIndex(e);
		return this.fns[n] ? (this.fns[n] = t, e) : !1;
	}
	use(e) {
		return this.fns.push(e), this.fns.length - 1;
	}
}, ke = () => ({
	error: new L(),
	request: new L(),
	response: new L()
}), Ae = N({
	allowReserved: !1,
	array: {
		explode: !0,
		style: "form"
	},
	object: {
		explode: !0,
		style: "deepObject"
	}
}), je = { "Content-Type": "application/json" }, R = (e = {}) => ({
	...ge,
	headers: je,
	parseAs: "auto",
	querySerializer: Ae,
	...e
}), z = ((e = {}) => {
	let t = F(R(), e), n = () => ({ ...t }), r = (e) => (t = F(t, e), n()), i = ke(), a = async (e) => {
		let n = {
			...t,
			...e,
			fetch: e.fetch ?? t.fetch ?? globalThis.fetch,
			headers: I(t.headers, e.headers),
			serializedBody: void 0
		};
		n.security && await De(n), n.requestValidator && await n.requestValidator(n), n.body !== void 0 && n.bodySerializer && (n.serializedBody = n.bodySerializer(n.body)), (n.body === void 0 || n.serializedBody === "") && n.headers.delete("Content-Type");
		let r = n;
		return {
			opts: r,
			url: P(r)
		};
	}, o = async (e) => {
		let n = e.throwOnError ?? t.throwOnError, r = e.responseStyle ?? t.responseStyle, o, s;
		try {
			let { opts: t, url: n } = await a(e), r = {
				redirect: "follow",
				...t,
				body: M(t)
			};
			o = new Request(n, r);
			for (let e of i.request.fns) e && (o = await e(o, t));
			let c = t.fetch;
			s = await c(o);
			for (let e of i.response.fns) e && (s = await e(s, o, t));
			let l = {
				request: o,
				response: s
			};
			if (s.ok) {
				let e = (t.parseAs === "auto" ? Te(s.headers.get("Content-Type")) : t.parseAs) ?? "json";
				if (s.status === 204 || s.headers.get("Content-Length") === "0") {
					let n;
					switch (e) {
						case "arrayBuffer":
						case "blob":
						case "text":
							n = await s[e]();
							break;
						case "formData":
							n = new FormData();
							break;
						case "stream":
							n = s.body;
							break;
						default:
							n = {};
							break;
					}
					return t.responseStyle === "data" ? n : {
						data: n,
						...l
					};
				}
				let n;
				switch (e) {
					case "arrayBuffer":
					case "blob":
					case "formData":
					case "text":
						n = await s[e]();
						break;
					case "json": {
						let e = await s.text();
						n = e ? JSON.parse(e) : {};
						break;
					}
					case "stream": return t.responseStyle === "data" ? s.body : {
						data: s.body,
						...l
					};
				}
				return e === "json" && (t.responseValidator && await t.responseValidator(n), t.responseTransformer && (n = await t.responseTransformer(n))), t.responseStyle === "data" ? n : {
					data: n,
					...l
				};
			}
			let u = await s.text(), d;
			try {
				d = JSON.parse(u);
			} catch {}
			throw d ?? u;
		} catch (t) {
			let a = t;
			for (let t of i.error.fns) t && (a = await t(a, s, o, e));
			if (a ||= {}, n) throw a;
			return r === "data" ? void 0 : {
				error: a,
				request: o,
				response: s
			};
		}
	}, s = (e) => (t) => o({
		...t,
		method: e
	}), c = (e) => async (t) => {
		let { opts: n, url: r } = await a(t);
		return _e({
			...n,
			body: n.body,
			method: e,
			onRequest: async (e, t) => {
				let r = new Request(e, t);
				for (let e of i.request.fns) e && (r = await e(r, n));
				return r;
			},
			serializedBody: M(n),
			url: r
		});
	};
	return {
		buildUrl: (e) => P({
			...t,
			...e
		}),
		connect: s("CONNECT"),
		delete: s("DELETE"),
		get: s("GET"),
		getConfig: n,
		head: s("HEAD"),
		interceptors: i,
		options: s("OPTIONS"),
		patch: s("PATCH"),
		post: s("POST"),
		put: s("PUT"),
		request: o,
		setConfig: r,
		sse: {
			connect: c("CONNECT"),
			delete: c("DELETE"),
			get: c("GET"),
			head: c("HEAD"),
			options: c("OPTIONS"),
			patch: c("PATCH"),
			post: c("POST"),
			put: c("PUT"),
			trace: c("TRACE")
		},
		trace: s("TRACE")
	};
})(R({
	baseUrl: "https://localhost:44324/",
	throwOnError: !0
})), Me = (e) => (e?.client ?? z).get({
	security: [{
		scheme: "bearer",
		type: "http"
	}],
	url: "/umbraco/search/api/v1/indexes",
	...e
}), Ne = (e) => (e.client ?? z).get({
	security: [{
		scheme: "bearer",
		type: "http"
	}],
	url: "/umbraco/search/api/v1/indexes/{indexAlias}",
	...e
}), Pe = (e) => (e?.client ?? z).put({
	security: [{
		scheme: "bearer",
		type: "http"
	}],
	url: "/umbraco/search/api/v1/rebuild",
	...e
}), Fe = (e) => (e.client ?? z).post({
	security: [{
		scheme: "bearer",
		type: "http"
	}],
	url: "/umbraco/search/api/v1/search",
	...e,
	headers: {
		"Content-Type": "application/json",
		...e.headers
	}
}), Ie = class {
	#e;
	constructor(e) {
		this.#e = e;
	}
	async createScaffold(e = {}) {
		let t = "idle";
		return e.healthStatus === "Rebuilding" ? t = "loading" : e.healthStatus === "Corrupted" && (t = "error"), { data: {
			entityType: i,
			name: e.indexAlias,
			providerName: e.providerName,
			unique: e.indexAlias,
			documentCount: 0,
			state: t,
			healthStatus: "Unknown",
			...e
		} };
	}
	async read(e) {
		if (!e) throw Error("Unique is missing");
		let { data: t, error: n } = await v(this.#e, Ne({
			client: g,
			path: { indexAlias: e }
		}));
		return n || !t ? { error: n } : this.createScaffold(t);
	}
	async create(e) {
		return console.error("Creating search indexes is not supported."), {
			data: e,
			error: new _("Creating search indexes is not supported")
		};
	}
	async update(e) {
		return console.error("Updating search indexes is not supported."), {
			data: e,
			error: new _("Updating search indexes is not supported")
		};
	}
	async delete(e) {
		return console.error("Deleting search indexes is not supported."), { error: new _("Deleting search indexes is not supported") };
	}
}, B = new m("UmbSearchDetailStore"), V = class extends te {
	#e;
	#t;
	#n = new f(this);
	constructor(e) {
		super(e, Ie, B), this.consumeContext(t, (e) => this.#e = e), this.consumeContext(p, (e) => this.#t = e);
	}
	async rebuildIndex(e) {
		this.#t?.peek("warning", { data: {
			title: this.#n.term("search_rebuildConfirmHeadline"),
			message: this.#n.term("search_rebuildStartedMessage", e)
		} });
		let { error: t } = await v(this, Pe({
			query: { indexAlias: e },
			client: g
		}));
		if (t) throw t;
		this.#e?.setUserWaitingForIndexUpdate(e, !0);
	}
	async save(e) {
		return console.error("Saving search indexes is not supported."), {
			data: e,
			error: void 0
		};
	}
}, H = class extends re {
	constructor(n) {
		super(n, e), this.consumeContext(t, (e) => {
			e && this.observe(e.indexRebuilt, (e) => {
				e && this.loadCollection();
			}, "index-rebuild-completed-observer");
		});
	}
	setIndexState(e, t) {
		this._items.updateOne(e, { state: t });
	}
}, U = new m("UmbWorkspaceContext", void 0, (e) => e.getEntityType?.() === i), Le = class extends ie {
	#e = new V(this);
	async execute() {
		if (!this.args.unique) throw Error("Index alias is not provided");
		await ae(this, {
			color: "warning",
			headline: "#search_rebuildConfirmHeadline",
			content: "#search_rebuildConfirmMessage",
			confirmLabel: "#search_rebuildConfirmLabel"
		});
		let e = await this.getContext(U).catch(() => void 0);
		e && e.setState("loading");
		let t = await this.getContext(b).catch(() => void 0);
		t instanceof H && t.setIndexState(this.args.unique, "loading"), await this.#e.rebuildIndex(this.args.unique);
	}
}, Re = class extends ne {
	async execute() {
		let e = await this.getContext(b);
		if (!e) throw Error("Collection context is not available");
		e.loadCollection();
	}
};
//#endregion
//#region \0@oxc-project+runtime@0.138.0/helpers/esm/decorate.js
function W(e, t, n, r) {
	var i = arguments.length, a = i < 3 ? t : r === null ? r = Object.getOwnPropertyDescriptor(t, n) : r, o;
	if (typeof Reflect == "object" && typeof Reflect.decorate == "function") a = Reflect.decorate(e, t, n, r);
	else for (var s = e.length - 1; s >= 0; s--) (o = e[s]) && (a = (i < 3 ? o(a) : i > 3 ? o(t, n, a) : o(t, n)) || a);
	return i > 3 && a && Object.defineProperty(t, n, a), a;
}
//#endregion
//#region src/settings/collection/search-root-collection-view.element.ts
var G = class extends x {
	#e;
	constructor() {
		super(), this._tableItems = [], this._tableConfig = { allowSelection: !1 }, this._tableColumns = [
			{
				name: this.localize.term("search_tableColumnAlias"),
				alias: "indexAlias"
			},
			{
				name: this.localize.term("search_tableColumnHealthStatus"),
				alias: "healthStatus"
			},
			{
				name: this.localize.term("search_tableColumnDocumentCount"),
				alias: "documentCount"
			},
			{
				name: "",
				alias: "entityActions",
				align: "right"
			}
		], this.consumeContext(b, (e) => {
			this.#e = e, this.#t();
		});
	}
	render() {
		return w`
      <umb-table
        .config=${this._tableConfig}
        .columns=${this._tableColumns}
        .items=${this._tableItems}
      ></umb-table>
    `;
	}
	#t() {
		this.observe(this.#e?.items, (e) => {
			this.isConnected && this.#n(e ?? []);
		}, "_itemsObserver");
	}
	#n(e) {
		this._tableItems = e?.map((e) => ({
			id: e.unique,
			icon: this.#r(e),
			data: [
				{
					columnAlias: "indexAlias",
					value: w`<a
              href=${`section/settings/workspace/${i}/edit/${e.unique}`}
              >${e.unique}</a
            >`
				},
				{
					columnAlias: "healthStatus",
					value: D(e.state === "loading", () => w`<uui-loader-bar></uui-loader-bar>`, () => this.localize.term("search_healthStatus", e.healthStatus))
				},
				{
					columnAlias: "documentCount",
					value: this.localize.term("search_documentCount", this.localize.number(e.documentCount))
				},
				{
					columnAlias: "entityActions",
					value: w`<umb-entity-actions-table-column-view
              .value=${{
						entityType: e.entityType,
						unique: e.unique,
						name: e.unique
					}}
            ></umb-entity-actions-table-column-view>`
				}
			]
		}));
	}
	#r(e) {
		if (e.state === "loading") return "icon-loading color-blue";
		switch (e.healthStatus) {
			case "Healthy": return "icon-check color-green";
			case "Rebuilding": return "icon-time color-yellow";
			case "Empty": return "icon-check color-yellow";
			default: return "icon-alert color-red";
		}
	}
	static {
		this.styles = [O];
	}
};
W([E()], G.prototype, "_tableItems", void 0), G = W([C("umb-search-root-collection-view")], G);
//#endregion
//#region src/settings/conditions/indexProviderName.condition.ts
var K = class extends oe {
	constructor(e, t) {
		super(e, t);
		let n = t.config.oneOf ?? (t.config.match ? [t.config.match] : void 0) ?? [];
		this.consumeContext(ce, (e) => {
			this.observe(e?.providerName, (e) => {
				this.permitted = e ? fe(n, e) : !1;
			}, "_observeProviderName");
		});
	}
}, ze = {
	type: "condition",
	name: "Search Index Provider Name Condition",
	alias: "Umb.Search.Condition.IndexProviderName",
	api: K
}, Be = class {
	#e;
	constructor(e) {
		this.#e = e;
	}
	async getCollection(e) {
		let { data: t, error: n } = await v(this.#e, Me({ client: g }));
		return n || !t ? { error: n } : { data: {
			items: t.items.map((e) => {
				let t = "idle";
				return e.healthStatus === "Rebuilding" ? t = "loading" : e.healthStatus === "Corrupted" && (t = "error"), {
					unique: e.indexAlias,
					name: e.indexAlias,
					providerName: e.providerName,
					documentCount: e.documentCount,
					healthStatus: e.healthStatus,
					entityType: i,
					state: t
				};
			}),
			total: t.total
		} };
	}
}, Ve = class extends y {
	#e = new Be(this);
	async requestCollection(e) {
		return this.#e.getCollection(e);
	}
}, He = class extends pe {
	constructor(e) {
		super(e, se.toString());
	}
}, Ue = class extends d {
	async search(e) {
		let t = {
			indexAlias: e.indexAlias,
			query: e.query ?? null,
			culture: e.culture ?? null
		}, { data: n, error: r } = await v(this, Fe({
			body: t,
			query: {
				skip: e.skip ?? 0,
				take: e.take ?? 10
			},
			client: g
		}));
		if (r || !n) return { error: r };
		let i = n.documents.map((e) => ({
			unique: e.id,
			objectType: String(e.objectType),
			entityType: this.#e(e.objectType),
			name: e.name ?? "Unknown",
			icon: e.icon ?? "icon-document"
		}));
		return { data: {
			total: n.total,
			documents: i
		} };
	}
	#e(e) {
		return {
			Document: "document",
			Media: "media",
			Member: "member",
			DocumentType: "document-type",
			MediaType: "media-type",
			MemberType: "member-type",
			DataType: "data-type"
		}[e] || e.toLowerCase();
	}
}, q = class extends y {
	#e = new Ue(this);
	async search(e) {
		return this.#e.search(e);
	}
}, J = class extends x {
	constructor() {
		super(), this.consumeContext(U, (e) => {
			this.observe(e?.state, (e) => {
				this._state = e ?? "idle";
			}, "_observeState");
		});
	}
	render() {
		return this._state === "loading" ? w`
        <div class="loading-state">
          <uui-loader></uui-loader>
          <span><umb-localize key="search_rebuildingIndex">Rebuilding index...</umb-localize></span>
        </div>
      ` : w`
      <div class="container">
        <div class="column">
          <umb-extension-slot
            type="searchIndexDetailBox"
            .filter=${(e) => e.meta?.column === "left"}
          ></umb-extension-slot>
        </div>
        <div class="column">
          <umb-extension-slot
            type="searchIndexDetailBox"
            .filter=${(e) => e.meta?.column !== "left"}
          ></umb-extension-slot>
        </div>
      </div>
    `;
	}
	static {
		this.styles = [O, S`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }

      .container {
        display: grid;
        grid-template-columns: 1fr 350px;
        gap: var(--uui-size-layout-1);
      }

      .column {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-layout-1);
      }

      .loading-state {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        min-height: 400px;
        gap: var(--uui-size-space-4);
      }
    `];
	}
};
W([E()], J.prototype, "_state", void 0), J = W([C("umb-search-details-view")], J);
//#endregion
//#region src/settings/workspace/search/views/search-index-search-box.element.ts
var Y = 10, X = class extends x {
	#e;
	#t;
	#n;
	#r;
	#i;
	#a;
	#o;
	#s;
	#c;
	constructor() {
		super(), this.#t = new q(this), this.#n = "", this.#i = de(() => {
			this.#p();
		}, 300), this.#a = new ue(), this._tableConfig = { allowSelection: !1 }, this._tableColumns = [
			{
				name: this.localize.term("search_tableColumnName"),
				alias: "name"
			},
			{
				name: this.localize.term("search_tableColumnEntityType"),
				alias: "entityType"
			},
			{
				name: "",
				alias: "actions",
				align: "right"
			}
		], this._tableItems = [], this._searchQuery = "", this._isSearching = !1, this._searchStatusMessage = "", this._currentPage = 1, this._totalPages = 1, this._languages = [], this._hasMultipleLanguages = !1, this.#l(), this.#a.setPageSize(Y), this.observe(this.#a.currentPage, (e) => {
			this._currentPage = e;
		}, "_observeCurrentPage"), this.observe(this.#a.totalPages, (e) => {
			this._totalPages = e;
		}, "_observeTotalPages"), new me(this, s).addAdditionalPath(":entityType").onSetup((e) => ({ data: {
			entityType: e.entityType,
			preset: {}
		} })).observeRouteBuilder((e) => {
			this.#r = e;
		}), this.consumeContext(U, (e) => {
			if (!e) return;
			this.#e = e;
			let t = this.#s ?? this.#c;
			t && !e.getSelectedCulture() && e.setSelectedCulture(t), this.observe(ee([e.name, e.selectedCulture]), ([e, t]) => {
				this._indexAlias = e ?? void 0, this._selectedCulture = t, e && t && this.#p();
			}, "_observeSearchReady"), this.#d();
		}), this.consumeContext(he, (e) => {
			e && (this.observe(e.languages, (e) => {
				this._languages = e, this._hasMultipleLanguages = e.length > 1;
			}, "_observeLanguages"), this.observe(e.appLanguageCulture, (e) => {
				this.#c = e, e && !this.#e?.getSelectedCulture() && this.#e?.setSelectedCulture(e);
			}, "_observeAppLanguageCulture"));
		});
	}
	#l() {
		let e = new URL(window.location.href), t = e.searchParams.get("query"), n = e.searchParams.get("page"), r = e.searchParams.get("culture");
		if (r && (this.#s = r), t && (this.#n = t, this._searchQuery = t), n) {
			let e = Number.parseInt(n, 10);
			!Number.isNaN(e) && e >= 1 && (this.#o = e);
		}
	}
	#u() {
		let e = new URL(window.location.href);
		this._searchQuery.trim() ? e.searchParams.set("query", this._searchQuery) : e.searchParams.delete("query");
		let t = this.#a.getCurrentPageNumber();
		t > 1 ? e.searchParams.set("page", String(t)) : e.searchParams.delete("page"), this._selectedCulture ? e.searchParams.set("culture", this._selectedCulture) : e.searchParams.delete("culture"), history.replaceState(null, "", e.toString());
	}
	#d() {
		this.observe(this.#e?.healthStatus, (e) => {
			this._healthStatus = e;
		}, "_observeHealthStatus");
	}
	get #f() {
		return this._healthStatus !== "Healthy";
	}
	render() {
		return w`
      <uui-box headline=${this.localize.term("search_searchBox")}>
        <div
          class="search-container"
          role="search"
          aria-label=${this.localize.term("search_searchFormLabel", this._indexAlias)}
          aria-busy=${this._isSearching ? "true" : "false"}
        >
          <!-- Screen reader status announcements -->
          <div class="visually-hidden" role="status" aria-live="polite" aria-atomic="true">
            ${this._searchStatusMessage}
          </div>

          ${D(this.#f, () => w`
              <div class="search-disabled-message">
                <umb-localize key="search_searchDisabled">
                  Search is disabled because the index is not healthy. Current status:
                </umb-localize>
                ${this.localize.term("search_healthStatus", this._healthStatus)}
              </div>
            `)}

          <div class="search-input-row">
            <uui-input
              id="search-input"
              .value=${this.#n}
              @input=${this.#g}
              @keydown=${this.#_}
              ?disabled=${this.#f}
              placeholder=${this.localize.term("search_searchPlaceholder")}
              label=${this.localize.term("search_searchInputLabel")}
              aria-label=${this.localize.term("search_searchInputAriaLabel", this._indexAlias)}
              aria-describedby="search-hint"
            >
              <uui-icon
                name="icon-search"
                slot="prepend"
                style="padding-left:var(--uui-size-space-2)"
              ></uui-icon>
            </uui-input>
            ${D(this._hasMultipleLanguages, () => w`
                <uui-select
                  label=${this.localize.term("search_cultureSelectLabel")}
                  .options=${this._languages.map((e) => ({
			name: e.name,
			value: e.unique,
			selected: e.unique === this._selectedCulture
		}))}
                  @change=${this.#y}
                ></uui-select>
              `)}
            <uui-button
              look="primary"
              color="positive"
              @click=${this.#v}
              ?disabled=${this.#f || this._isSearching}
              label=${this.localize.term("search_searchButtonAriaLabel")}
            >
              <umb-localize key="search_searchButton">Search</umb-localize>
            </uui-button>
          </div>

          <div id="search-hint" class="visually-hidden">
            <umb-localize key="search_searchHint">
              Press Enter or click Search button to execute search
            </umb-localize>
          </div>
          ${D(this._isSearching, () => w`
              <div role="status" aria-label=${this.localize.term("search_loading")}>
                <uui-loader></uui-loader>
              </div>
            `)}
          ${D(this._error, () => w`
              <div class="error-message" role="alert" aria-live="assertive">${this._error}</div>
            `)}
          ${this.#x()}
        </div>
      </uui-box>
    `;
	}
	async #p() {
		if (this._isSearching || (this._searchQuery = this.#n, !this._indexAlias)) return;
		this._isSearching = !0, this._error = void 0, this._searchStatusMessage = this.localize.term("search_searching");
		let e = this.#o;
		this.#o = void 0;
		let t = e ? (e - 1) * Y : this.#a.getSkip(), n = {
			indexAlias: this._indexAlias,
			query: this._searchQuery,
			culture: this._selectedCulture,
			skip: t,
			take: Y
		}, { data: r, error: i } = await this.#t.search(n);
		i || !r ? (this._error = i?.message ?? this.localize.term("search_searchError"), this._searchResults = void 0, this._tableItems = [], this._searchStatusMessage = this.localize.term("search_searchFailed")) : (this._searchResults = r, this.#a.setTotalItems(r.total), e && this.#a.setCurrentPageNumber(e), this.#m(r), this._searchStatusMessage = this.localize.term("search_searchComplete", r.total)), this._isSearching = !1, this.#u();
	}
	#m(e) {
		this._tableItems = e.documents.map((e) => ({
			id: `${e.unique}_${this._selectedCulture}`,
			icon: e.icon,
			data: [
				{
					columnAlias: "name",
					value: w`
            <div style="padding: var(--uui-size-2) 0;">
              <uui-button
                look="secondary"
                label="Open"
                aria-label=${this.localize.term("search_openEntity", e.entityType, e.unique)}
                href=${this.#h(e.unique, e.entityType)}
              >
                ${e.name}
              </uui-button>
              <div><small>${e.unique}</small></div>
            </div>
          `
				},
				{
					columnAlias: "entityType",
					value: e.entityType
				},
				{
					columnAlias: "actions",
					value: w`<umb-entity-actions-table-column-view
            .value=${{
						unique: e.unique,
						entityType: r,
						name: e.name
					}}
          ></umb-entity-actions-table-column-view>`
				}
			]
		}));
	}
	#h(e, t) {
		return this.#r ? `${this.#r({ entityType: t })}edit/${e}` : (console.error("Route builder not initialized"), "#");
	}
	#g(e) {
		let t = e.target;
		this.#n = t.value, this.#a.setCurrentPageNumber(1), this.#i();
	}
	#_(e) {
		e.key === "Enter" && this.#p();
	}
	#v() {
		this.#p();
	}
	#y(e) {
		let t = e.target;
		this.#a.setCurrentPageNumber(1), this.#e?.setSelectedCulture(t.value);
	}
	#b(e) {
		let t = e.target;
		this.#a.setCurrentPageNumber(t.current), this.#p();
	}
	#x() {
		return this._searchResults ? this._searchResults.total === 0 ? w`
        <div class="no-results" role="status" aria-live="polite">
          <umb-localize key="search_noResults">No results found</umb-localize>
        </div>
      ` : w`
      <div
        class="results-container"
        role="region"
        aria-label=${this.localize.term("search_resultsRegion")}
      >
        <div class="results-header" id="results-summary">
          <strong>
            <umb-localize key="search_resultsCount" .args=${[this._searchResults.total]}>
              Found ${this._searchResults.total} result${this._searchResults.total === 1 ? "" : "s"}
            </umb-localize>
          </strong>
        </div>
        <umb-table
          .config=${this._tableConfig}
          .columns=${this._tableColumns}
          .items=${this._tableItems}
          aria-describedby="results-summary"
          aria-label=${this.localize.term("search_resultsTable")}
        >
        </umb-table>
        ${this._totalPages > 1 ? w`
                <uui-pagination
                  .current=${this._currentPage}
                  .total=${this._totalPages}
                  ?disabled=${this._isSearching}
                  @change=${this.#b}
                  aria-label=${this.localize.term("search_paginationLabel")}
                ></uui-pagination>
              ` : T}
      </div>
    ` : T;
	}
	static {
		this.styles = [O, S`
      :host {
        display: block;
      }

      .search-container {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-4);
      }

      /* Visually hidden but accessible to screen readers */
      .visually-hidden {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border-width: 0;
      }

      .search-input-row {
        display: flex;
        gap: var(--uui-size-space-3);
        align-items: flex-end;
      }

      uui-input {
        flex: 1;
      }

      .error-message {
        padding: var(--uui-size-space-4);
        background-color: var(--uui-color-danger-standalone);
        color: var(--uui-color-danger-contrast);
        border-radius: var(--uui-border-radius);
      }

      .search-disabled-message {
        color: var(--uui-color-danger);
        font-size: 0.875rem;
      }

      .no-results {
        padding: var(--uui-size-space-4);
        text-align: center;
        color: var(--uui-color-text-alt);
      }

      .results-container {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-3);
      }

      .results-header {
        padding-bottom: var(--uui-size-space-2);
        border-bottom: 1px solid var(--uui-color-border);
        margin-bottom: var(--uui-size-space-3);
      }

      uui-pagination {
        display: flex;
        justify-content: center;
      }
    `];
	}
};
W([E()], X.prototype, "_tableItems", void 0), W([E()], X.prototype, "_indexAlias", void 0), W([E()], X.prototype, "_healthStatus", void 0), W([E()], X.prototype, "_searchQuery", void 0), W([E()], X.prototype, "_searchResults", void 0), W([E()], X.prototype, "_isSearching", void 0), W([E()], X.prototype, "_error", void 0), W([E()], X.prototype, "_searchStatusMessage", void 0), W([E()], X.prototype, "_currentPage", void 0), W([E()], X.prototype, "_totalPages", void 0), W([E()], X.prototype, "_languages", void 0), W([E()], X.prototype, "_selectedCulture", void 0), W([E()], X.prototype, "_hasMultipleLanguages", void 0), X = W([C("umb-search-index-search-box")], X);
//#endregion
//#region src/settings/workspace/search/views/search-index-stats-box.element.ts
var Z = class extends x {
	#e;
	constructor() {
		super(), this.consumeContext(U, (e) => {
			this.#e = e, this.#t();
		});
	}
	#t() {
		this.observe(this.#e?.name, (e) => {
			this._indexAlias = e;
		}, "_observeName"), this.observe(this.#e?.providerName, (e) => {
			this._providerName = e;
		}, "_observeProviderName"), this.observe(this.#e?.documentCount, (e) => {
			this._documentCount = e;
		}, "_observeDocumentCount"), this.observe(this.#e?.healthStatus, (e) => {
			this._healthStatus = e;
		}, "_observeHealthStatus");
	}
	#n(e) {
		switch (e) {
			case "Healthy": return "positive";
			case "Rebuilding":
			case "Empty": return "warning";
			case "Corrupted": return "danger";
			default: return "default";
		}
	}
	render() {
		return w`
      <uui-box headline=${this.localize.term("search_indexInfo")}>
        <div class="stats-grid">
          <div class="stat-item">
            <strong><umb-localize key="search_indexAlias">Index Alias</umb-localize></strong>
            <span>${this._indexAlias ?? "—"}</span>
          </div>

          <div class="stat-item">
            <strong><umb-localize key="search_providerName">Provider Name</umb-localize></strong>
            <span>${this._providerName ?? "—"}</span>
          </div>

          <div class="stat-item">
            <strong>
              <umb-localize key="search_tableColumnDocumentCount"> Document Count </umb-localize>
            </strong>
            <span>${this.localize.term("search_documentCount", this._documentCount)}</span>
          </div>

          <div class="stat-item">
            <strong>
              <umb-localize key="search_tableColumnHealthStatus"> Health Status </umb-localize>
            </strong>
            <div>
              <uui-tag look="primary" .color=${this.#n(this._healthStatus)}>
                ${this.localize.term("search_healthStatus", this._healthStatus)}
              </uui-tag>
            </div>
          </div>
        </div>
      </uui-box>
    `;
	}
	static {
		this.styles = [O, S`
      :host {
        display: block;
      }

      .stats-grid {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-5);
      }

      .stat-item {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-2);
      }
    `];
	}
};
W([E()], Z.prototype, "_indexAlias", void 0), W([E()], Z.prototype, "_providerName", void 0), W([E()], Z.prototype, "_documentCount", void 0), W([E()], Z.prototype, "_healthStatus", void 0), Z = W([C("umb-search-index-stats-box")], Z);
//#endregion
//#region src/settings/workspace/paths.ts
var Q = c.generateAbsolute({
	sectionName: u,
	entityType: a
}), $ = class extends x {
	#e;
	constructor() {
		super(), this.consumeContext(U, (e) => {
			this.#e = e, this.#t();
		});
	}
	#t() {
		this.observe(this.#e?.name, (e) => {
			this._indexAlias = e;
		}, "_observeIndexAlias");
	}
	render() {
		return w`
      <umb-entity-detail-workspace-editor .backPath=${Q}>
        <h3 slot="header">${this._indexAlias ?? "Loading..."}</h3>
      </umb-entity-detail-workspace-editor>
    `;
	}
	static {
		this.styles = [O];
	}
};
W([E()], $.prototype, "_indexAlias", void 0), $ = W([C("umb-search-workspace-editor")], $);
//#endregion
//#region src/settings/workspace/search/search-workspace.context.ts
var We = class extends l {
	#e;
	getSelectedCulture() {
		return this.#e.getValue();
	}
	setSelectedCulture(e) {
		this.#e.setValue(e);
	}
	constructor(e) {
		super(e, {
			workspaceAlias: o,
			entityType: i,
			detailRepositoryAlias: n
		}), this.repository = new le(this), this.documentCount = this._data.createObservablePartOfPersisted((e) => e?.documentCount), this.healthStatus = this._data.createObservablePartOfPersisted((e) => e?.healthStatus), this.providerName = this._data.createObservablePartOfPersisted((e) => e?.providerName), this.state = this._data.createObservablePartOfCurrent((e) => e?.state), this.#e = new h(void 0), this.selectedCulture = this.#e.asObservable(), this.routes.setRoutes([{
			path: "edit/:unique",
			component: $,
			setup: (e, t) => {
				this.load(t.match.params.unique);
			}
		}]), this.consumeContext(t, (e) => {
			this.observe(e?.indexRebuilt, (e) => {
				e && e === this.getUnique() && this.reload();
			}, "index-rebuild-completed-detail-observer");
		});
	}
	setState(e) {
		this._data.updateCurrent({ state: e });
	}
};
//#endregion
export { B as UMB_SEARCH_DETAIL_STORE_CONTEXT, Q as UMB_SEARCH_ROOT_WORKSPACE_PATH, U as UMB_SEARCH_WORKSPACE_CONTEXT, H as UmbSearchCollectionContext, Re as UmbSearchCollectionReloadAction, Ve as UmbSearchCollectionRepository, V as UmbSearchDetailRepository, He as UmbSearchDetailStore, K as UmbSearchIndexProviderNameCondition, q as UmbSearchQueryRepository, Ue as UmbSearchQueryServerDataSource, Le as UmbSearchRebuildIndexEntityAction, We as UmbSearchWorkspaceContext, ze as manifest };

//# sourceMappingURL=search-settings.js.map