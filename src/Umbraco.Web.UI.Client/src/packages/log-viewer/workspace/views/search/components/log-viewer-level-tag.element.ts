import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import { html, LitElement, ifDefined, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';

interface LevelMapStyles {
	look?: UUIInterfaceLook;
	color?: UUIInterfaceColor;
	style?: string;
}

@customElement('umb-log-viewer-level-tag')
export class UmbLogViewerLevelTagElement extends LitElement {
	@property()
	level?: LogLevelModel;

	levelMap: Record<LogLevelModel, LevelMapStyles> = {
		Verbose: { look: 'secondary' },
		Debug: {
			look: 'default',
			style: 'background-color: var(--umb-log-viewer-debug-color); color: var(--umb-log-viewer-debug-color-contrast',
		},
		Information: { look: 'primary', color: 'positive' },
		Warning: { look: 'primary', color: 'warning' },
		Error: { look: 'primary', color: 'danger' },
		Fatal: {
			look: 'primary',
			style: 'background-color: var(--umb-log-viewer-fatal-color); color: var(--umb-log-viewer-fatal-color-contrast)',
		},
	};

	override render() {
		return html`<uui-tag
			look=${ifDefined(this.level ? this.levelMap[this.level]?.look : undefined)}
			color=${ifDefined(this.level ? this.levelMap[this.level]?.color : undefined)}
			style="${ifDefined(this.level ? this.levelMap[this.level]?.style : undefined)}"
			>${this.level}<slot></slot
		></uui-tag>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-level-tag': UmbLogViewerLevelTagElement;
	}
}
