import type { UmbEntitySignElement } from '../../types.js';
import type { ManifestEntitySignIconKind } from './types.js';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-sign-icon')
export class UmbEntitySignIconElement extends UmbLitElement implements UmbEntitySignElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestEntitySignIconKind;

	override render() {
		return this.manifest
			? html`<umb-icon
					.name=${this.manifest.meta.iconName ?? 'icon-circle-dotted'}
					.color=${this.manifest.meta.iconColorAlias}></umb-icon>`
			: nothing;
	}

	static override styles = [
		css`
			umb-icon {
				filter: drop-shadow(-1px 0 0 var(--umb-sign-bundle-bg)) drop-shadow(0 -1px 0 var(--umb-sign-bundle-bg))
					drop-shadow(0 1px 0 var(--umb-sign-bundle-bg));
			}
			umb-icon::before {
				content: '';
				position: absolute;
				z-index: -1;
				border-radius: 50%;
				inset: 2px;
				background-color: var(--umb-sign-bundle-bg);
			}
		`,
	];
}

export { UmbEntitySignIconElement as element };
