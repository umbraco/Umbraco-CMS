import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import type { UmbUserStartNodesModel } from '../../../../types.js';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../user-workspace-assign-access/user-workspace-assign-access.element.js';

const elementName = 'umb-user-workspace-access';
@customElement(elementName)
export class UmbUserWorkspaceAccessElement extends UmbLitElement {
	@state()
	private _calculatedStartNodes?: UmbUserStartNodesModel;

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(
				this.#userWorkspaceContext?.calculatedStartNodes,
				(calculatedStartNodes) => (this._calculatedStartNodes = calculatedStartNodes),
				'umbUserObserver',
			);
		});
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('user_access')}>
				<div slot="header" class="faded-text">
					<umb-localize key="user_accessHelp"
						>Based on the assigned groups and start nodes, the user has access to the following nodes</umb-localize
					>
				</div>
				<div>
					${this.#renderDocumentStartNodes()} ${this.#renderMediaStartNodes()} ${this.#renderElementStartNodes()}
				</div>
			</uui-box>
		`;
	}

	#renderDocumentStartNodes() {
		const uniques = this._calculatedStartNodes?.documentStartNodeUniques.map((reference) => reference.unique) || [];
		return html`
			<umb-property-layout label=${this.localize.term('sections_content')} orientation="vertical">
				<div slot="editor">
					<umb-user-document-start-node readonly .uniques=${uniques}></umb-user-document-start-node>
				</div>
			</umb-property-layout>
		`;
	}

	#renderElementStartNodes() {
		const uniques = this._calculatedStartNodes?.elementStartNodeUniques.map((reference) => reference.unique) || [];
		return html`
			<umb-property-layout label=${this.localize.term('general_elements')} orientation="vertical">
				<div slot="editor">
					<umb-user-element-start-node readonly .uniques=${uniques}></umb-user-element-start-node>
				</div>
			</umb-property-layout>
		`;
	}

	#renderMediaStartNodes() {
		const uniques = this._calculatedStartNodes?.mediaStartNodeUniques.map((reference) => reference.unique) || [];
		return html`
			<umb-property-layout label=${this.localize.term('sections_media')} orientation="vertical">
				<div slot="editor">
					<umb-user-media-start-node readonly .uniques=${uniques}></umb-user-media-start-node>
				</div>
			</umb-property-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				width: 100%;
			}
			.faded-text {
				color: var(--uui-color-text-alt);
				font-size: 0.8rem;
			}
		`,
	];
}

export default UmbUserWorkspaceAccessElement;

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserWorkspaceAccessElement;
	}
}
