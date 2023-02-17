import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { DocumentPropertyModel, PropertyTypeContainerViewModelBaseModel } from '@umbraco-cms/backend-api';

@customElement('umb-document-workspace-view-edit-tab')
export class UmbDocumentWorkspaceViewEditTabElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];

	private _tabName?: string | undefined;

	@property({ type: String })
	public get tabName(): string | undefined {
		return this._tabName;
	}
	public set tabName(value: string | undefined) {
		if (this._tabName === value) return;
		this._tabName = value;
		this._observeContainers();
	}

	@state()
	_propertyData: DocumentPropertyModel[] = [];

	@state()
	_groups: PropertyTypeContainerViewModelBaseModel[] = [];

	_propertiesObservables: Map<string, unknown> = new Map();

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbDocumentWorkspaceContext>('umbWorkspaceContext', (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			this._observeContainers();
			//this._observeContent();
		});
	}

	private _observeProperties() {
		if (!this._workspaceContext) return;

		/*
		Just get the properties for the current containers. (and eventually variants later)
		*/
		this.observe(
			this._workspaceContext.propertyValuesOf(null, null),
			(properties) => {
				this._propertyData = properties || [];
				//this._data = content?.data || [];

				/*
				Maybe we should not give the value(Data), but the umb-content-property should get the context and observe its own data.
				This would become a more specific Observer therefor better performance?.. Note to self: Debate with Mads how he sees this perspective.
				*/
			},
			'observeWorkspaceContextData'
		);
	}

	private _observeContainers() {
		if (!this._workspaceContext) return;

		this.observe(
			this._workspaceContext.containersOf(this.tabName, 'Group'),
			(groups) => {
				this._groups = groups || [];
			},
			'observeWorkspaceContextData'
		);
	}

	private _getPropertiesOfGroup(group: PropertyTypeContainerViewModelBaseModel) {
		if (!this._workspaceContext) return undefined;

		this.observe(
			this._workspaceContext.propertyValuesOf(null, null),
			(properties) => {
				this._propertyData = properties || [];
				//this._data = content?.data || [];

				/*
				Maybe we should not give the value(Data), but the umb-content-property should get the context and observe its own data.
				This would become a more specific Observer therefor better performance?.. Note to self: Debate with Mads how he sees this perspective.
				*/
			},
			'observeWorkspaceContextData'
		);

		// cache observable
	}

	render() {
		return 'hello worlds' + this._tabName;
		/*repeat(
					this._groups,
					(group) => group.key,
					(group) =>
						html`
							<uui-box>
								${repeat(
									this._getPropertiesOfGroup(group),
									(property) => property.alias,
									(property) =>
										html`<umb-content-property
											.property=${property}
											.value=${this._propertyData.find((x) => x.alias === property.alias)?.value}></umb-content-property> `
								)}
							</uui-box>
						`
					)
		);*/
	}
}

export default UmbDocumentWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-tab': UmbDocumentWorkspaceViewEditTabElement;
	}
}
