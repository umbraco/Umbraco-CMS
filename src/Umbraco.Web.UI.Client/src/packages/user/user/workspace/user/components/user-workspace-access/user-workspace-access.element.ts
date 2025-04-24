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
		return html` <uui-box headline=${this.localize.term('user_access')}>
			<div slot="header" class="faded-text">
				<umb-localize key="user_accessHelp"
					>Based on the assigned groups and start nodes, the user has access to the following nodes</umb-localize
				>
			</div>

			${this.#renderDocumentStartNodes()}
			<hr />
			${this.#renderMediaStartNodes()}
		</uui-box>`;
	}

	#renderDocumentStartNodes() {
		return html` <b><umb-localize key="sections_content">Content</umb-localize></b>
			<umb-user-document-start-node
				readonly
				.uniques=${this._calculatedStartNodes?.documentStartNodeUniques.map((reference) => reference.unique) ||
				[]}></umb-user-document-start-node>`;
	}

	#renderMediaStartNodes() {
		return html` <b><umb-localize key="sections_media">Media</umb-localize></b>
			<umb-user-media-start-node
				readonly
				.uniques=${this._calculatedStartNodes?.mediaStartNodeUniques.map((reference) => reference.unique) ||
				[]}></umb-user-media-start-node>`;
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
