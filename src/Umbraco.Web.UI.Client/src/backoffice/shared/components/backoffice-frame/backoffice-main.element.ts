import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section/section.context';
import { UmbSectionElement } from '../section/section.element';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestSection } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

@defineElement('umb-backoffice-main')
export class UmbBackofficeMain extends UmbLitElement {
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

	constructor() {
		super();

		this._observeSections();
	}

	private async _observeSections() {

		this.observe(umbExtensionsRegistry.extensionsOfType('section'), (sections) => {
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
				setup: this._onRouteSetup, // TODO: sometimes we can end up in a state where this callback doesn't get called. It could look like a bug in the router-slot.
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
		this._provideSectionContext(section);
	};

	private _provideSectionContext(section: ManifestSection) {
		if (!this._sectionContext) {
			this._sectionContext = new UmbSectionContext(section);
			this.provideContext(UMB_SECTION_CONTEXT_TOKEN, this._sectionContext);
		} else {
			this._sectionContext.setManifest(section);
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
