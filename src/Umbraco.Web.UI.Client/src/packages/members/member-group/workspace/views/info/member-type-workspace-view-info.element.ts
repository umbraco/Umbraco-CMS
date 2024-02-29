// import { UMB_COMPOSITION_PICKER_MODAL, type UmbCompositionPickerModalData } from '../../../modals/index.js';
import type { UmbMemberGroupWorkspaceContext } from '../../member-group-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-member-type-workspace-view-member-info')
export class UmbMemberTypeWorkspaceViewMemberInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	private _workspaceContext?: UmbMemberGroupWorkspaceContext;

	@state()
	private _unique = '';

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, async (context) => {
			this._workspaceContext = context as UmbMemberGroupWorkspaceContext;
			this._unique = this._workspaceContext.getUnique();
		});
	}

	render() {
		return html` <div id="left-column">
				<uui-box headline="Member Group">
					<div id="no-properties">
						<umb-localize key="member_memberGroupNoProperties"></umb-localize>
					</div>
				</uui-box>
			</div>

			<div id="right-column">
				<uui-box headline="General">
					<div class="property">
						<b>Id</b>
						<span>${this._unique}</span>
					</div>
				</uui-box>
			</div>`;
	}

	static styles = [
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
