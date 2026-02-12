import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_WORKSPACE_VIEW_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';

@customElement('example-hint-workspace-view')
export class ExampleHintWorkspaceView extends UmbElementMixin(LitElement) {
	//

	async onClick() {
		/*
		const context = await this.getContext(UMB_WORKSPACE_VIEW_NAVIGATION_CONTEXT);
		if (!context) {
			throw new Error('Could not find the context');
		}
		const view = await context.getViewContext('example.workspaceView.hint');
		*/

		/*
		const view = await this.getContext(UMB_WORKSPACE_VIEW_CONTEXT);
		if (!view) {
			throw new Error('Could not find the view');
		}

		if (view.hints.has('exampleHintFromToggleAction')) {
			view.hints.removeOne('exampleHintFromToggleAction');
		} else {
			view.hints.addOne({
				unique: 'exampleHintFromToggleAction',
				text: 'Hi',
				color: 'invalid',
				weight: 100,
			});
		}
			*/

		const workspace = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		if (!workspace) {
			throw new Error('Could not find the workspace');
		}

		if (workspace.view.hints.has('exampleHintFromToggleAction')) {
			workspace.view.hints.removeOne('exampleHintFromToggleAction');
		} else {
			workspace.view.hints.addOne({
				unique: 'exampleHintFromToggleAction',
				path: ['Umb.WorkspaceView.Document.Edit'],
				text: 'Hi',
				color: 'invalid',
				weight: 100,
			});
		}
	}

	override render() {
		return html`
			<uui-box class="uui-text">
				<h1 class="uui-h2" style="margin-top: var(--uui-size-layout-1);">See the hint on this views tab</h1>
				<p>This is toggle on/off via this button:</p>
				<uui-button type="button" @click=${this.onClick} look="primary" color="positive">Toggle hint</uui-button>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export { ExampleHintWorkspaceView as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-hint-workspace-view': ExampleHintWorkspaceView;
	}
}
