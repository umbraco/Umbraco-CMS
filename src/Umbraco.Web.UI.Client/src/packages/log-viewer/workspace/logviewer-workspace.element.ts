// TODO: I don't feel sure this is good, seems wrong: [NL]
import '../components/index.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-logviewer-workspace')
export class UmbLogViewerWorkspaceElement extends UmbLitElement {
	override render() {
		return html`
			<umb-workspace-editor
				headline=${this.localize.term('treeHeaders_logViewer')}
				.enforceNoFooter=${true}></umb-workspace-editor>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;

				--umb-log-viewer-debug-color: color-mix(in srgb, var(--uui-color-default-emphasis) 75%, var(--uui-color-text));
				--umb-log-viewer-information-color: color-mix(in srgb, var(--uui-color-positive) 75%, var(--uui-color-text));
				--umb-log-viewer-warning-color: color-mix(
					in oklch,
					color-mix(in oklch, var(--uui-color-warning) 70%, var(--uui-color-danger)) 75%,
					var(--uui-color-text)
				);
				--umb-log-viewer-error-color: color-mix(in srgb, var(--uui-color-danger) 80%, var(--uui-color-text));
				--umb-log-viewer-fatal-color: var(--uui-color-text);
				--umb-log-viewer-verbose-color: color-mix(in srgb, var(--uui-color-text) 55%, var(--uui-color-surface));
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}
		`,
	];
}

export { UmbLogViewerWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-workspace': UmbLogViewerWorkspaceElement;
	}
}
