import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyDatasetElement, UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { PropertyEditorSettingsProperty, UmbStartNodeAccessValue } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-user-workspace-assign-access')
export class UmbUserWorkspaceAssignAccessElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _values: Array<UmbPropertyValueData> = [
		{ alias: 'userGroups', value: [] },
		{ alias: 'documentAccess', value: { rootAccess: false, startNodes: [] } },
		{ alias: 'mediaAccess', value: { rootAccess: false, startNodes: [] } },
		{ alias: 'elementAccess', value: { rootAccess: false, startNodes: [] } },
		{ alias: 'documentBlueprintAccess', value: { rootAccess: false, startNodes: [] } },
	];

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			if (!instance) return;

			this.observe(
				instance.userGroupUniques,
				(value) => this.#setValue('userGroups', value ?? []),
				'_observeUserGroupAccess',
			);

			this.observe(
				observeMultiple([instance.hasDocumentRootAccess, instance.documentStartNodeUniques]),
				([rootAccess, startNodes]) =>
					this.#setValue('documentAccess', { rootAccess: rootAccess ?? false, startNodes: startNodes ?? [] }),
				'_observeDocumentAccess',
			);

			this.observe(
				observeMultiple([instance.hasMediaRootAccess, instance.mediaStartNodeUniques]),
				([rootAccess, startNodes]) =>
					this.#setValue('mediaAccess', { rootAccess: rootAccess ?? false, startNodes: startNodes ?? [] }),
				'_observeMediaAccess',
			);

			this.observe(
				observeMultiple([instance.hasElementRootAccess, instance.elementStartNodeUniques]),
				([rootAccess, startNodes]) =>
					this.#setValue('elementAccess', { rootAccess: rootAccess ?? false, startNodes: startNodes ?? [] }),
				'_observeElementAccess',
			);

			this.observe(
				observeMultiple([instance.hasDocumentBlueprintRootAccess, instance.documentBlueprintStartNodeUniques]),
				([rootAccess, startNodes]) =>
					this.#setValue('documentBlueprintAccess', { rootAccess: rootAccess ?? false, startNodes: startNodes ?? [] }),
				'_observeDocumentBlueprintAccess',
			);
		});
	}

	#setValue(alias: string, value: unknown) {
		this._values = this._values.map((entry) => (entry.alias === alias ? { alias, value } : entry));
	}

	#onChange(event: Event & { target: UmbPropertyDatasetElement }) {
		const values = event.target.value;

		const userGroups = values.find((entry) => entry.alias === 'userGroups')?.value as
			| Array<UmbReferenceByUnique>
			| undefined;
		this.#workspaceContext?.setUserGroups(userGroups ?? []);

		const documentAccess = values.find((entry) => entry.alias === 'documentAccess')?.value as
			| UmbStartNodeAccessValue
			| undefined;
		if (documentAccess) this.#workspaceContext?.setDocumentAccess(documentAccess);

		const mediaAccess = values.find((entry) => entry.alias === 'mediaAccess')?.value as
			| UmbStartNodeAccessValue
			| undefined;
		if (mediaAccess) this.#workspaceContext?.setMediaAccess(mediaAccess);

		const elementAccess = values.find((entry) => entry.alias === 'elementAccess')?.value as
			| UmbStartNodeAccessValue
			| undefined;
		if (elementAccess) this.#workspaceContext?.setElementAccess(elementAccess);

		const documentBlueprintAccess = values.find((entry) => entry.alias === 'documentBlueprintAccess')?.value as
			| UmbStartNodeAccessValue
			| undefined;
		if (documentBlueprintAccess) this.#workspaceContext?.setDocumentBlueprintAccess(documentBlueprintAccess);
	}

	#getFields(): Array<PropertyEditorSettingsProperty> {
		return [
			{
				alias: 'userGroups',
				label: this.localize.term('general_groups'),
				description: this.localize.term('user_groupsHelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.UserGroupPicker',
			},
			{
				alias: 'documentAccess',
				label: this.localize.term('user_startnodes'),
				description: this.localize.term('user_startnodeshelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentStartNodeAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllDocuments') }],
			},
			{
				alias: 'mediaAccess',
				label: this.localize.term('defaultdialogs_selectMediaStartNode'),
				description: this.localize.term('user_mediastartnodehelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaStartNodeAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllMedia') }],
			},
			{
				alias: 'elementAccess',
				label: this.localize.term('user_selectElementStartNode'),
				description: this.localize.term('user_selectElementStartNodeDescription'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.ElementStartNodeAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllElements') }],
			},
			{
				alias: 'documentBlueprintAccess',
				label: this.localize.term('user_selectDocumentBlueprintStartNode'),
				description: this.localize.term('user_selectDocumentBlueprintStartNodeDescription'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentBlueprintStartNodeAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllDocumentBlueprints') }],
			},
		];
	}

	override render() {
		return html`
			<uui-box .headline=${this.localize.term('user_assignAccess')}>
				<umb-property-dataset .value=${this._values} @change=${this.#onChange}>
					${repeat(
						this.#getFields(),
						(field) => field.alias,
						(field) => html`
							<umb-property
								alias=${field.alias}
								label=${field.label}
								description=${ifDefined(field.description)}
								property-editor-ui-alias=${field.propertyEditorUiAlias}
								.config=${field.config}>
							</umb-property>
						`,
					)}
				</umb-property-dataset>
			</uui-box>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-assign-access': UmbUserWorkspaceAssignAccessElement;
	}
}
