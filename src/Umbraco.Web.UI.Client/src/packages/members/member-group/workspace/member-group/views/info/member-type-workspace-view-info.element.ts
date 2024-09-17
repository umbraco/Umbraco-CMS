// import { UMB_COMPOSITION_PICKER_MODAL, type UmbCompositionPickerModalData } from '../../../modals/index.js';
import { UMB_MEMBER_GROUP_WORKSPACE_CONTEXT } from '../../member-group-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-member-type-workspace-view-member-info')
export class UmbMemberTypeWorkspaceViewMemberInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	private _workspaceContext?: typeof UMB_MEMBER_GROUP_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _unique = '';

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_GROUP_WORKSPACE_CONTEXT, async (context) => {
			this._workspaceContext = context;
			this._unique = this._workspaceContext.getUnique() ?? '';
		});
	}

	override render() {
		return html` <div id="left-column">
				<uui-box headline=${this.localize.term('content_membergroup')}>
					<div id="no-properties">
						<umb-localize key="member_memberGroupNoProperties">
							Member groups have no additional properties for editing.
						</umb-localize>
					</div>
				</uui-box>
			</div>

			<div id="right-column">
				<uui-box headline=${this.localize.term('general_general')}>
					<div class="property">
						<b><umb-localize key="general_id">Id</umb-localize></b>
						<span>${this._unique}</span>
					</div>
				</uui-box>
			</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#no-properties {
				display: flex;
				justify-content: center;
				align-items: center;
				color: var(--uui-color-text);
				opacity: 0.5;
			}
			#left-column {
				/* Is there a way to make the wrapped right column grow only when wrapped? */
				flex: 9999 1 500px;
			}
			#right-column {
				flex: 1 1 350px;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			:host {
				display: flex;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-4);
			}
			.property {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbMemberTypeWorkspaceViewMemberInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace-view-member-info': UmbMemberTypeWorkspaceViewMemberInfoElement;
	}
}
