import type { UmbMenuItemElement } from '../../menu-item-element.interface.js';
import type { ManifestMenuItem } from '../../menu-item.extension.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';

@customElement('umb-menu-item-default')
export class UmbMenuItemDefaultElement extends UmbLitElement implements UmbMenuItemElement {
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

	override render() {
		return html`
			<umb-menu-item-layout
				.href=${this._href}
				.iconName=${this.manifest.meta.icon ?? ''}
				.label=${this.localize.string(this.manifest.meta.label ?? this.manifest.name)}
				.entityType=${this.manifest.meta.entityType}>
			</umb-menu-item-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-default': UmbMenuItemDefaultElement;
	}
}
