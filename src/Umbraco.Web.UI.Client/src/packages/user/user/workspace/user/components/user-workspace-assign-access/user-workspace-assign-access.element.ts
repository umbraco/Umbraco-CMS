import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import type { UmbUserDetailModel } from '../../../../types.js';
import { css, customElement, html, when, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbInputEntityDataElement } from '@umbraco-cms/backoffice/entity-data-picker';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbUserGroupInputElement } from '@umbraco-cms/backoffice/user-group';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';

import '@umbraco-cms/backoffice/entity-data-picker';

const elementName = 'umb-user-workspace-assign-access';
@customElement(elementName)
export class UmbUserWorkspaceAssignAccessElement extends UmbLitElement {
	@state()
	private _userGroupUniques: UmbUserDetailModel['userGroupUniques'] = [];

	@state()
	private _documentStartNodeUniques: UmbUserDetailModel['documentStartNodeUniques'] = [];

	@state()
	private _documentRootAccess: UmbUserDetailModel['hasDocumentRootAccess'] = false;

	@state()
	private _elementStartNodeUniques: UmbUserDetailModel['elementStartNodeUniques'] = [];

	@state()
	private _elementRootAccess: UmbUserDetailModel['hasElementRootAccess'] = false;

	@state()
	private _mediaStartNodeUniques: UmbUserDetailModel['mediaStartNodeUniques'] = [];

	@state()
	private _mediaRootAccess: UmbUserDetailModel['hasMediaRootAccess'] = false;

	#workspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;

			this.observe(
				this.#workspaceContext?.userGroupUniques,
				(value) => (this._userGroupUniques = value ?? []),
				'_observeUserGroupAccess',
			);

			this.observe(
				this.#workspaceContext?.hasDocumentRootAccess,
				(value) => (this._documentRootAccess = value ?? false),
				'_observeDocumentRootAccess',
			);

			this.observe(
				this.#workspaceContext?.documentStartNodeUniques,
				(value) => (this._documentStartNodeUniques = value ?? []),
				'_observeDocumentStartNode',
			);

			this.observe(
				this.#workspaceContext?.hasElementRootAccess,
				(value) => (this._elementRootAccess = value ?? false),
				'_observeElementRootAccess',
			);

			this.observe(
				this.#workspaceContext?.elementStartNodeUniques,
				(value) => (this._elementStartNodeUniques = value ?? []),
				'_observeElementStartNode',
			);

			this.observe(
				this.#workspaceContext?.hasMediaRootAccess,
				(value) => (this._mediaRootAccess = value ?? false),
				'_observeMediaRootAccess',
			);

			this.observe(
				this.#workspaceContext?.mediaStartNodeUniques,
				(value) => (this._mediaStartNodeUniques = value ?? []),
				'_observeMediaStartNode',
			);
		});
	}

	#onUserGroupsChange(event: CustomEvent) {
		event.stopPropagation();
		const target = event.target as UmbUserGroupInputElement;
		const selection: Array<UmbReferenceByUnique> = target.selection.map((unique) => {
			return { unique };
		});
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('userGroupUniques', selection);
	}

	#onAllowAllDocumentsChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.target;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('hasDocumentRootAccess', target.checked);
		this.#workspaceContext?.updateProperty('documentStartNodeUniques', []);
	}

	#onDocumentStartNodeChange(event: CustomEvent) {
		event.stopPropagation();
		// TODO: get back to this when media have been decoupled from users.
		// The event target is deliberately set to any to avoid an import cycle with media.
		const target = event.target as any;
		const selection: Array<UmbReferenceByUnique> = target.selection.map((unique: string) => ({ unique }));
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('documentStartNodeUniques', selection);
		// When specific start nodes are selected, disable root access
		this.#workspaceContext?.updateProperty('hasDocumentRootAccess', false);
	}

	#onAllowAllElementsChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.target;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('hasElementRootAccess', target.checked);
		this.#workspaceContext?.updateProperty('elementStartNodeUniques', []);
	}

	#onElementStartNodeChange(event: CustomEvent & { target: UmbInputEntityDataElement }) {
		event.stopPropagation();
		// TODO: get back to this when media have been decoupled from users.
		// The event target is deliberately set to any to avoid an import cycle with media.
		const target = event.target;
		const selection: Array<UmbReferenceByUnique> = target.selection.map((unique: string) => ({ unique }));
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('elementStartNodeUniques', selection);
		// When specific start nodes are selected, disable root access
		this.#workspaceContext?.updateProperty('hasElementRootAccess', false);
	}

	#onAllowAllMediaChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.target;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('hasMediaRootAccess', target.checked);
		this.#workspaceContext?.updateProperty('mediaStartNodeUniques', []);
	}

	#onMediaStartNodeChange(event: CustomEvent) {
		event.stopPropagation();
		// TODO: get back to this when media have been decoupled from users.
		// The event target is deliberately set to any to avoid an import cycle with media.
		const target = event.target as any;
		const selection: Array<UmbReferenceByUnique> = target.selection.map((unique: string) => ({ unique }));
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('mediaStartNodeUniques', selection);
		// When specific start nodes are selected, disable root access
		this.#workspaceContext?.updateProperty('hasMediaRootAccess', false);
	}

	override render() {
		return html`
			<uui-box>
				<div slot="headline"><umb-localize key="user_assignAccess">Assign Access</umb-localize></div>
				<div id="assign-access">
					${this.#renderGroupAccess()} ${this.#renderDocumentAccess()} ${this.#renderMediaAccess()}
					${this.#renderElementAccess()}
				</div>
			</uui-box>
		`;
	}

	#renderGroupAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('general_groups')}
				description=${this.localize.term('user_groupsHelp')}>
				<umb-user-group-input
					slot="editor"
					.selection=${this._userGroupUniques.map((reference) => reference.unique)}
					@change=${this.#onUserGroupsChange}></umb-user-group-input>
			</umb-property-layout>
		`;
	}

	#renderDocumentAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('user_startnodes')}
				description=${this.localize.term('user_startnodeshelp')}>
				<div slot="editor">
					<uui-toggle
						style="margin-bottom: var(--uui-size-space-3);"
						label=${this.localize.term('user_allowAccessToAllDocuments')}
						.checked=${this._documentRootAccess}
						@change=${this.#onAllowAllDocumentsChange}></uui-toggle>
				</div>
				${when(
					this._documentRootAccess === false,
					() => html`
						<umb-input-document
							slot="editor"
							.selection=${this._documentStartNodeUniques.map((reference) => reference.unique)}
							@change=${this.#onDocumentStartNodeChange}>
						</umb-input-document>
					`,
				)}
			</umb-property-layout>
		`;
	}

	#renderElementAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('user_selectElementStartNode')}
				description=${this.localize.term('user_selectElementStartNodeDescription')}>
				<div slot="editor">
					<uui-toggle
						style="margin-bottom: var(--uui-size-space-3);"
						label=${this.localize.term('user_allowAccessToAllElements')}
						.checked=${this._elementRootAccess}
						@change=${this.#onAllowAllElementsChange}></uui-toggle>
				</div>
				${when(
					this._elementRootAccess === false,
					() => html`
						<umb-input-entity-data
							slot="editor"
							.selection=${this._elementStartNodeUniques.map((reference) => reference.unique)}
							.dataSourceAlias=${'Umb.PropertyEditorDataSource.ElementFolder'}
							.dataSourceConfig=${[]}
							@change=${this.#onElementStartNodeChange}>
						</umb-input-entity-data>
					`,
				)}
			</umb-property-layout>
		`;
	}

	#renderMediaAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('defaultdialogs_selectMediaStartNode')}
				description=${this.localize.term('user_mediastartnodehelp')}>
				<div slot="editor">
					<uui-toggle
						style="margin-bottom: var(--uui-size-space-3);"
						label=${this.localize.term('user_allowAccessToAllMedia')}
						.checked=${this._mediaRootAccess}
						@change=${this.#onAllowAllMediaChange}></uui-toggle>
				</div>
				${when(
					this._mediaRootAccess === false,
					() => html`
						<umb-input-media
							slot="editor"
							.selection=${this._mediaStartNodeUniques.map((reference) => reference.unique)}
							@change=${this.#onMediaStartNodeChange}>
						</umb-input-media>
					`,
				)}
			</umb-property-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserWorkspaceAssignAccessElement;
	}
}
