import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { Subscription } from 'rxjs';

import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../core/context';
import { createExtensionElement } from '../../core/extension';
import type { ManifestSection } from '../../core/models';
import { UmbSectionStore } from '../../core/stores/section.store';
import { UmbSectionContext } from '../sections/section.context';

@defineElement('umb-backoffice-main')
export class UmbBackofficeMain extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				background-color: var(--uui-color-background);
				display: block;
				width: 100%;
				height: 100%;
				overflow: hidden;
			}
		`,
	];

	@state()
	private _routes: Array<any> = [];

	@state()
	private _sections: Array<ManifestSection> = [];

	private _routePrefix = 'section/';
	private _sectionContext?: UmbSectionContext;
	private _sectionStore?: UmbSectionStore;
	private _sectionSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbSectionStore', (_instance: UmbSectionStore) => {
			this._sectionStore = _instance;
			this._useSections();
		});
	}

	private async _useSections() {
		this._sectionSubscription?.unsubscribe();

		this._sectionSubscription = this._sectionStore?.getAllowed().subscribe((sections) => {
			if (!sections) return;
			this._sections = sections;
			this._createRoutes();
		});
	}

	private _createRoutes() {
		this._routes = [];
		this._routes = this._sections.map((section) => {
			return {
				path: this._routePrefix + section.meta.pathname,
				component: () => createExtensionElement(section),
				setup: this._onRouteSetup,
			};
		});

		this._routes.push({
			path: '**',
			redirectTo: this._routePrefix + this._sections?.[0]?.meta.pathname,
		});
	}

	private _onRouteSetup = (_component: HTMLElement, info: IRoutingInfo) => {
		const currentPath = info.match.route.path;
		const section = this._sections.find((s) => this._routePrefix + s.meta.pathname === currentPath);
		if (!section) return;
		this._sectionStore?.setCurrent(section.alias);
		this._provideSectionContext(section);
	};

	private _provideSectionContext(section: ManifestSection) {
		if (!this._sectionContext) {
			this._sectionContext = new UmbSectionContext(section);
			this.provideContext('umbSectionContext', this._sectionContext);
		} else {
			this._sectionContext.update(section);
		}
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._sectionSubscription?.unsubscribe();
	}

	render() {
		return html`<router-slot .routes=${this._routes}></router-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-main': UmbBackofficeMain;
	}
}
