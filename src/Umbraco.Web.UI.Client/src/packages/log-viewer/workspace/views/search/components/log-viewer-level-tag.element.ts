import { html, LitElement, customElement, nothing, property, styleMap } from '@umbraco-cms/backoffice/external/lit';
import type { LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-log-viewer-level-tag')
export class UmbLogViewerLevelTagElement extends LitElement {
	@property()
	level?: LogLevelModel;

	#tagStyle() {
		if (!this.level) return nothing;
		const levelColor = `var(--umb-log-viewer-${this.level.toLowerCase()}-color)`;
		return styleMap({
			'--uui-tag-border-color': levelColor,
			backgroundColor: `color-mix(in srgb, ${levelColor} 18%, var(--uui-color-surface))`,
			color: 'var(--uui-color-text)',
		});
	}

	override render() {
		return html`<uui-tag look="secondary" style=${this.#tagStyle()}>${this.level}<slot></slot></uui-tag>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-level-tag': UmbLogViewerLevelTagElement;
	}
}
