import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section/section.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extensions-registry';

@customElement('umb-menu-item')
export class UmbMenuItemElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Object, attribute: false })
	manifest!: ManifestMenuItem;

	@state()
	private _href?: string;

	#sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this.#sectionContext = sectionContext;
			this._observeSection();
		});
	}

	private _observeSection() {
		if (!this.#sectionContext) return;

		this.observe(this.#sectionContext?.pathname, (pathname) => {
			if (!pathname) return;
			this._href = this._constructPath(pathname);
		});
	}

	// TODO: how do we handle this?
	private _constructPath(sectionPathname: string) {
		return `section/${sectionPathname}/workspace/${this.manifest.meta.entityType}`;
	}

	render() {
		return html` <uui-menu-item href="${ifDefined(this._href)}" .label=${this.manifest.meta.label || this.manifest.name}
			><uui-icon slot="icon" name=${this.manifest.meta.icon}></uui-icon
		></uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item': UmbMenuItemElement;
	}
}
