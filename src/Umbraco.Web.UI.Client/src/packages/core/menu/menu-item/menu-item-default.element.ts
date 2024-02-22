import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestMenuItem, UmbMenuItemElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';

@customElement('umb-menu-item-default')
export class UmbMenuItemDefaultElement extends UmbLitElement implements UmbMenuItemElement {
	//
	#pathname?: string;

	@property({ type: Object, attribute: false })
	private _manifest!: ManifestMenuItem;
	public get manifest(): ManifestMenuItem {
		return this._manifest;
	}
	public set manifest(value: ManifestMenuItem) {
		this._manifest = value;
		this.#constructHref();
	}

	@state()
	private _href?: string;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT, (sectionContext) => {
			this.observe(
				sectionContext?.pathname,
				(pathname) => {
					this.#pathname = pathname;
					this.#constructHref();
				},
				'observePathname',
			);
		});
	}

	#constructHref() {
		if (!this.#pathname || !this.manifest) return;
		this._href = `section/${this.#pathname}/workspace/${this.manifest.meta.entityType}`;
	}

	render() {
		return html`<umb-menu-item-layout
			.label=${this.manifest.meta.label ?? this.manifest.name}
			.iconName=${this.manifest.meta.icon ?? ''}
			.href=${this._href}></umb-menu-item-layout>`;
	}

	static styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-default': UmbMenuItemDefaultElement;
	}
}
