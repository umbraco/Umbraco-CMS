import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import '../../../../../shared/property-creator/property-creator.element.ts';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-document-type-workspace-view-design')
export class UmbDocumentTypeWorkspaceViewDesignElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
			/* TODO: This should be replaced with a general workspace bar — naming is hard */
			#workspace-tab-bar {
				padding: 0 var(--uui-size-layout-1);
				display: flex;
				align-items: center;
				justify-content: space-between;
				background-color: var(--uui-color-surface);
				flex-wrap: nowrap;
			}
			.tab-actions {
				display: flex;
				gap: var(--uui-size-space-4);
			}
			.tab-actions uui-button uui-icon {
				padding-right: calc(-1 * var(--uui-size-space-4));
			}

			uui-tab {
				display: flex;
				flex-direction: row;
				flex-wrap: nowrap;
			}

			uui-tab .trash {
				display: flex;
				align-items: stretch;
			}

			uui-tab uui-input {
				flex-grow: 1;
			}

			uui-input:not(:focus) {
				border: 1px solid transparent;
			}

			uui-input:not(:hover, :focus) .trash {
				opacity: 0;
			}

			/** Property Group Wrapper */

			#wrapper {
				margin: var(--uui-size-layout-1);
			}

			#add-group {
				margin-top: var(--uui-size-layout-1);
				width: 100%;
				--uui-button-height: var(--uui-size-layout-4);
			}

			.group-headline {
				display: flex;
				gap: var(--uui-size-space-4);
			}
			.group-headline uui-input {
				flex-grow: 1;
			}
		`,
	];

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	@state()
	private _tabs: any[] = [];

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this._workspaceContext = documentTypeContext as UmbWorkspaceDocumentTypeContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this._workspaceContext) return;
	}

	render() {
		return html`
			<div id="workspace-tab-bar">
				${this.renderTabBar()}
				<div class="tab-actions">
					<uui-button label="Compositions" look="outline" compact>
						<uui-icon name="umb:merge"></uui-icon>
						Compositions
					</uui-button>
					<uui-button label="Recorder" look="outline" compact>
						<uui-icon name="umb:navigation"></uui-icon>
						Recorder
					</uui-button>
				</div>
			</div>
			<div id="wrapper">
				<uui-box class="group-wrapper">
					<div class="group-headline" slot="headline">
						<uui-input label="Group name" value="${''}" size="10">
							<uui-button slot="append" label="Delete group" compact><uui-icon name="umb:trash"></uui-icon></uui-button>
						</uui-input>
					</div>
					<umb-property-creator></umb-property-creator>
				</uui-box>
				<uui-button label="Add group" id="add-group" look="placeholder">Add group</uui-button>
			</div>
		`;
	}

	#remove(index: number) {
		this._tabs.splice(index, 1);
		this.requestUpdate();
	}
	async #addTab() {
		this._tabs = [...this._tabs, { name: 'Test' }];
	}

	renderTabBar() {
		return html`<uui-tab-group>
			${repeat(
				this._tabs,
				(tab) => tab.name,
				(tab, index) => {
					//TODO: Should these tabs be part of routing?
					return html`<uui-tab label=${tab.name!} .active="${false}">
						<div>
							<uui-input label="${tab.name}" look="placeholder" value="" placeholder="Enter a name">
								<uui-button
									label="Remove tab"
									class="trash"
									slot="append"
									@click="${() => this.#remove(index)}"
									compact>
									<uui-icon name="umb:trash"></uui-icon>
								</uui-button>
							</uui-input>
						</div>
					</uui-tab>`;
				}
			)}
			<uui-button id="add-tab" @click="${this.#addTab}" label="Add tab" compact>
				<uui-icon name="umb:add"></uui-icon>
				Add tab
			</uui-button>
		</uui-tab-group>`;
	}
}

export default UmbDocumentTypeWorkspaceViewDesignElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-design': UmbDocumentTypeWorkspaceViewDesignElement;
	}
}
