import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentWorkspaceContext } from '../../document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import './document-workspace-view-edit-properties.element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-document-workspace-view-edit-tab')
export class UmbDocumentWorkspaceViewEditTabElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			uui-box + uui-box {
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];

	private _tabName?: string | undefined;

	@property({ type: String })
	public get tabName(): string | undefined {
		return this._tabName;
	}
	public set tabName(value: string | undefined) {
		const oldValue = this._tabName;
		if (oldValue === value) return;
		this._tabName = value;
		this._observeTabContainers();
		this.requestUpdate('tabName', oldValue);
	}

	private _noTabName = false;

	@property({ type: Boolean })
	public get noTabName(): boolean {
		return this._noTabName;
	}
	public set noTabName(value: boolean) {
		const oldValue = this._noTabName;
		if (oldValue === value) return;
		this._noTabName = value;
		if (this._noTabName) {
			this._tabName = undefined;
		}
		this._observeTabContainers();
		this.requestUpdate('noTabName', oldValue);
	}

	@state()
	_tabContainers: PropertyTypeContainerResponseModelBaseModel[] = [];

	@state()
	_hasTabProperties = false;

	@state()
	_groups: Array<PropertyTypeContainerResponseModelBaseModel> = [];

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._workspaceContext = workspaceContext as UmbDocumentWorkspaceContext;
			this._observeTabContainers();
		});
	}

	private _observeHasTabProperties() {
		if (!this._workspaceContext) return;

		this._tabContainers.forEach((container) => {
			this.observe(
				this._workspaceContext!.structure.hasPropertyStructuresOf(container.key!),
				(hasTabProperties) => {
					this._hasTabProperties = hasTabProperties;
				},
				'_observeHasTabProperties_' + container.key
			);
		});
	}

	private _observeTabContainers() {
		if (!this._workspaceContext) return;

		if (this._tabName) {
			this._groups = [];
			this.observe(
				this._workspaceContext.structure.containersByNameAndType(this._tabName, 'Tab'),
				(tabContainers) => {
					this._tabContainers = tabContainers || [];
					if (this._tabContainers.length > 0) {
						this._observeHasTabProperties();
						this._observeGroups();
					}
				},
				'_observeTabContainers'
			);
		} else if (this._noTabName) {
			this._groups = [];
			this._observeRootGroups();
		}
	}

	private _observeGroups() {
		if (!this._workspaceContext || !this._tabName) return;

		this._tabContainers.forEach((container) => {
			this.observe(
				this._workspaceContext!.structure.containersOfParentKey(container.key, 'Group'),
				this._insertGroupContainers,
				'_observeGroupsOf_' + container.key
			);
		});
	}

	private _observeRootGroups() {
		if (!this._workspaceContext || !this._noTabName) return;

		// This is where we potentially could observe root properties as well.
		this.observe(
			this._workspaceContext!.structure.rootContainers('Group'),
			this._insertGroupContainers,
			'_observeRootGroups'
		);
	}

	private _insertGroupContainers = (groupContainers: PropertyTypeContainerResponseModelBaseModel[]) => {
		groupContainers.forEach((group) => {
			if (group.name) {
				if (!this._groups.find((x) => x.name === group.name)) {
					this._groups.push(group);
					this._groups.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
				}
			}
		});
	};

	render() {
		// TODO: only show tab properties if there was any. We might do this with an event? to tell us when the properties is empty.
		return html`
			${this._hasTabProperties
				? html`
						<uui-box>
							<umb-document-workspace-view-edit-properties
								container-type="Tab"
								container-name=${this.tabName || ''}></umb-document-workspace-view-edit-properties>
						</uui-box>
				  `
				: ''}
			${repeat(
				this._groups,
				(group) => group.name,
				(group) => html`<uui-box .headline=${group.name || ''}>
					<umb-document-workspace-view-edit-properties
						container-type="Group"
						container-name=${group.name || ''}></umb-document-workspace-view-edit-properties>
				</uui-box>`
			)}
		`;
	}
}

export default UmbDocumentWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-tab': UmbDocumentWorkspaceViewEditTabElement;
	}
}
