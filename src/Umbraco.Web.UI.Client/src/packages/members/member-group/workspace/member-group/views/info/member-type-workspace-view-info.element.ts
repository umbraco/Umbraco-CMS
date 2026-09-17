import { UMB_MEMBER_GROUP_WORKSPACE_CONTEXT } from '../../member-group-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UmbEntityStateEntry } from '@umbraco-cms/backoffice/entity-state';

@customElement('umb-member-type-workspace-view-member-info')
export class UmbMemberTypeWorkspaceViewMemberInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	private _workspaceContext?: typeof UMB_MEMBER_GROUP_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _unique = '';

	@state()
	private _entityStates: Array<UmbEntityStateEntry> = [];

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_GROUP_WORKSPACE_CONTEXT, async (context) => {
			this._workspaceContext = context;
			this._unique = this._workspaceContext?.getUnique() ?? '';
			this.observe(this._workspaceContext?.entityState.states, (states) => {
				this._entityStates = states ?? [];
			});
		});
	}

	#renderEntityStateTags() {
		if (!this._entityStates.length) return nothing;
		return html`
			<div class="property">
				<b><umb-localize key="general_status">Status</umb-localize></b>
				<umb-entity-state-tags .states=${this._entityStates}></umb-entity-state-tags>
			</div>
		`;
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
					${this.#renderEntityStateTags()}
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

			.property umb-entity-state-tags {
				display: inline-flex;
				flex-wrap: wrap;
				align-items: center;
				gap: var(--uui-size-space-1);
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
