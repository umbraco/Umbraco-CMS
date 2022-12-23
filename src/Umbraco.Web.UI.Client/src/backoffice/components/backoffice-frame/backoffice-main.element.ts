import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbSectionStore } from '../../../core/stores/section.store';
import { UmbSectionContext } from '../../test/core/components/section/section.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { ManifestSection } from '@umbraco-cms/models';
import { UmbSectionElement } from 'src/backoffice/test/core/components/section/section.element';

@defineElement('umb-backoffice-main')
export class UmbBackofficeMain extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbObserverMixin(LitElement))) {
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
			router-slot {
				height: 100%;
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

	constructor() {
		super();

		this.consumeContext('umbSectionStore', (_instance: UmbSectionStore) => {
			this._sectionStore = _instance;
			this._observeSections();
		});
	}

	private async _observeSections() {
		if (!this._sectionStore) return;

		this.observe<ManifestSection[]>(this._sectionStore?.getAllowed(), (sections) => {
			this._sections = sections;
			if (!sections) return;
			this._createRoutes();
		});
	}

	private _createRoutes() {
		this._routes = [];
		this._routes = this._sections.map((section) => {
			return {
				path: this._routePrefix + section.meta.pathname,
				component: () => this._getSectionElement(section),
				setup: this._onRouteSetup,
			};
		});

		this._routes.push({
			path: '**',
			redirectTo: this._routePrefix + this._sections?.[0]?.meta.pathname,
		});
	}

	// TODO: Make this a common shared method on @umbraco-cms/extensions-api
	private _getSectionElement(section: ManifestSection) {
		if (!section.loader || !section.elementName || !section.js) {
			return UmbSectionElement;
		}

		return createExtensionElement(section);
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

	render() {
		return html`<router-slot .routes=${this._routes}></router-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-main': UmbBackofficeMain;
	}
}
