import { UUIIconRequestEvent, UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-ref-manifest')
export class UmbRefManifestElement extends UmbElementMixin(UUIRefNodeElement) {
	@property({ type: Object, attribute: false })
	public get manifest(): ManifestBase | undefined {
		return undefined;
	}
	public set manifest(value: ManifestBase | undefined) {
		this._alias = value?.alias;
		this.name = value?.name ?? '';
	}

	@state()
	private _alias?: string;

	override connectedCallback() {
		super.connectedCallback();

		this.#requestIconSVG('icon-umb-manifest');
	}

	/* This is a bit stupid, but because this element extends from uui-ref-node, it only accepts the icon via the icon slot.
	 ** Instead we overwrite the fallbackIcon property which requires a SVG... */
	#requestIconSVG(iconName: string) {
		if (iconName !== '' && iconName !== null) {
			const event = new UUIIconRequestEvent(UUIIconRequestEvent.ICON_REQUEST, {
				detail: { iconName: iconName },
			});
			this.dispatchEvent(event);
			if (event.icon !== null) {
				event.icon.then((iconSvg: string) => {
					this.fallbackIcon = iconSvg;
					this.requestUpdate('fallbackIcon');
				});
			}
		}
	}

	protected override renderDetail() {
		return html`<small id="detail">${this._alias}<slot name="detail"></slot></small>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-manifest': UmbRefManifestElement;
	}
}
