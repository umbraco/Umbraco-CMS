import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';
import { LogLevelModel } from '@umbraco-cms/backend-api';

interface LevelMapStyles {
	look?: InterfaceLook;
	color?: InterfaceColor;
	style?: string;
}

@customElement('umb-log-viewer-level-tag')
export class UmbLogViewerLevelTagElement extends LitElement {
	static styles = [UUITextStyles, css``];

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
		Fatal: { look: 'primary' },
	};

	render() {
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
