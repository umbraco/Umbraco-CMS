import { UUITextStyles, InterfaceColor, InterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import { css, html, LitElement, ifDefined, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { LogLevelModel } from '@umbraco-cms/backoffice/backend-api';

interface LevelMapStyles {
	look?: InterfaceLook;
	color?: InterfaceColor;
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
			style: 'background-color: var(--umb-log-viewer-debug-color); color: var(--uui-color-surface)',
		},
		Information: { look: 'primary', color: 'positive' },
		Warning: { look: 'primary', color: 'warning' },
		Error: { look: 'primary', color: 'danger' },
		Fatal: {
			look: 'primary',
			style: 'background-color: var(--umb-log-viewer-fatal-color); color: var(--uui-color-surface)',
		},
	};

	render() {
		return html`<uui-tag
			look=${ifDefined(this.level ? this.levelMap[this.level]?.look : undefined)}
			color=${ifDefined(this.level ? this.levelMap[this.level]?.color : undefined)}
			style="${ifDefined(this.level ? this.levelMap[this.level]?.style : undefined)}"
			>${this.level}<slot></slot
		></uui-tag>`;
	}

	static styles = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-level-tag': UmbLogViewerLevelTagElement;
	}
}
