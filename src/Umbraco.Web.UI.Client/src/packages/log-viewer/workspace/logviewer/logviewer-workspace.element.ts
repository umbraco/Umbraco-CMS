// TODO: Niels: I don't feel sure this is good, seems wrong:
import '../../components/index.js';
import { UmbLogViewerWorkspaceContext } from '../logviewer.context.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDefaultWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

//TODO make uui-input accept min and max values
@customElement('umb-logviewer-workspace')
export class UmbLogViewerWorkspaceElement extends UmbLitElement {
	// TODO: This context is not used as the API, it needs some refactoring to be able to be used as a workspace api: [NL]
	#logViewerContext = new UmbLogViewerWorkspaceContext(this);

	firstUpdated(props: PropertyValueMap<unknown>) {
		super.firstUpdated(props);

		// TODO: This should be moved to the log viewer context:
		window.addEventListener('changestate', this.#logViewerContext.onChangeState);
		this.#logViewerContext.onChangeState();
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		window.removeEventListener('changestate', this.#logViewerContext.onChangeState);
	}

	load(): void {
		// Not relevant for this workspace -added to prevent the error from popping up
	}

	create(): void {
		// Not relevant for this workspace
	}

	render() {
		return html`
			<umb-workspace-editor
				headline=${this.localize.term('treeHeaders_logViewer')}
				.enforceNoFooter=${true}></umb-workspace-editor>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;

				--umb-log-viewer-debug-color: var(--uui-color-default-emphasis);
				--umb-log-viewer-information-color: var(--uui-color-positive);
				--umb-log-viewer-warning-color: var(--uui-color-warning);
				--umb-log-viewer-error-color: var(--uui-color-danger);
				--umb-log-viewer-fatal-color: var(--uui-palette-black);
				--umb-log-viewer-verbose-color: var(--uui-color-current);
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

// TODO: This does have its own context, but it needs some refactoring to be able to be used as a standalone workspace context: [NL]
export { UmbDefaultWorkspaceContext as api };

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-workspace': UmbLogViewerWorkspaceElement;
	}
}
