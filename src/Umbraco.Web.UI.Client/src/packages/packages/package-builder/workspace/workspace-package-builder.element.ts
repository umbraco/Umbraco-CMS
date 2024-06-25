import { UmbDictionaryPickerContext } from '../../../dictionary/components/input-dictionary/input-dictionary.context.js';
import { UmbPackageRepository } from '../../package/repository/index.js';
import { UmbPartialViewPickerContext } from '../../../templating/partial-views/components/input-partial-view/input-partial-view.context.js';
import { UmbScriptPickerContext } from '../../../templating/scripts/components/input-script/input-script.context.js';
import { UmbStylesheetPickerContext } from '../../../templating/stylesheets/components/stylesheet-input/stylesheet-input.context.js';
import { UmbTemplatePickerContext } from '../../../templating/templates/components/input-template/input-template.context.js';
import type { UmbCreatedPackageDefinition } from '../../types.js';
import type { UmbDataTypeInputElement } from '../../../data-type/components/data-type-input/data-type-input.element.js';
import type { UmbInputLanguageElement } from '../../../language/components/input-language/input-language.element.js';
import {
	css,
	html,
	customElement,
	property,
	query,
	state,
	when,
	nothing,
	ifDefined,
} from '@umbraco-cms/backoffice/external/lit';
import { blobDownload } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UmbInputDocumentTypeElement } from '@umbraco-cms/backoffice/document-type';
import type { UmbInputEntityElement } from '@umbraco-cms/backoffice/components';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UmbInputMediaTypeElement } from '@umbraco-cms/backoffice/media-type';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type {
	UUIBooleanInputEvent,
	UUIButtonState,
	UUIInputElement,
	UUIInputEvent,
} from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-workspace-package-builder')
export class UmbWorkspacePackageBuilderElement extends UmbLitElement {
	@property()
	entityUnique?: string;

	@property()
	workspaceAlias?: string;

	@state()
	private _package?: UmbCreatedPackageDefinition;

	@query('#package-name-input')
	private _packageNameInput?: UUIInputElement;

	@state()
	private _submitState?: UUIButtonState;

	#notificationContext?: UmbNotificationContext;
	#packageRepository = new UmbPackageRepository(this);
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#getPackageCreated();
	}

	async #getPackageCreated() {
		this._package = await this.#packageRepository.getCreatedPackage(this.entityUnique);
		this.requestUpdate('_package');
	}

	async #download() {
		if (!this._package?.unique) return;

		const data = await this.#packageRepository.getCreatePackageDownload(this._package.unique);
		if (!data) return;

		const filename = typeof data === 'object' ? 'package.zip' : 'package.xml';
		const mimeType = typeof data === 'object' ? 'application/zip' : 'text/xml';
		blobDownload(data, filename, mimeType);
	}

	#isNameDefined() {
		const valid = this._packageNameInput?.checkValidity() ?? false;
		if (!valid) this.#notificationContext?.peek('danger', { data: { message: 'Package missing a name' } });
		return valid;
	}

	async #save() {
		if (!this.#isNameDefined()) return;
		if (!this._package) return;

		this._submitState = 'waiting';

		const unique = await this.#packageRepository.saveCreatedPackage(this._package);
		if (!unique) return;

		this._package.unique = unique;
		this.requestUpdate('_package');

		this._submitState = 'success';

		this.#notificationContext?.peek('positive', { data: { message: 'Package saved' } });
	}

	async #update() {
		if (!this.#isNameDefined()) return;
		if (!this._package?.unique) return;

		this._submitState = 'waiting';

		const success = await this.#packageRepository.updateCreatedPackage(this._package);
		if (!success) return;

		this._submitState = 'success';

		this.#notificationContext?.peek('positive', { data: { message: 'Package updated' } });
	}

	override render() {
		if (!this.workspaceAlias) return nothing;
		return html`
			<umb-workspace-editor alias=${this.workspaceAlias} back-path="section/packages/view/created">
				${this.#renderHeader()} ${this.#renderEditors()} ${this.#renderActions()}
			</umb-workspace-editor>
		`;
	}

	#renderHeader() {
		if (!this._package) return nothing;
		return html`
			<div id="header" slot="header">
				<uui-input
					id="package-name-input"
					required
					label="Name of the package"
					placeholder=${this.localize.term('placeholders_entername')}
					${umbFocus()}
					.value=${this._package?.name ?? ''}
					@input=${(e: UUIInputEvent) => (this._package!.name = e.target.value as string)}></uui-input>
			</div>
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				${when(
					this._package?.unique,
					() => html`
						<uui-button
							color="default"
							look="secondary"
							label=${this.localize.term('general_download')}
							@click=${this.#download}></uui-button>
					`,
				)}
				<uui-button
					color="positive"
					look="primary"
					state=${ifDefined(this._submitState)}
					label=${this._package?.unique ? 'Update' : 'Create'}
					@click=${this._package?.unique ? this.#update : this.#save}></uui-button>
			</div>
		`;
	}

	#renderEditors() {
		return html`
			<uui-box headline="Package Content">
				${this.#renderDocumentSection()} ${this.#renderMediaSection()} ${this.#renderDocumentTypeSection()}
				${this.#renderMediaTypeSection()} ${this.#renderLanguageSection()} ${this.#renderDictionarySection()}
				${this.#renderDataTypeSection()} ${this.#renderTemplateSection()} ${this.#renderStylesheetsSection()}
				${this.#renderScriptsSection()} ${this.#renderPartialViewSection()}
			</uui-box>
		`;
	}

	#onContentChange(event: { target: UmbInputDocumentElement }) {
		if (!this._package) return;

		this._package.contentNodeId = event.target.selection[0];

		if (this._package.contentLoadChildNodes && !this._package.contentNodeId) {
			this._package.contentLoadChildNodes = false;
		}

		this.requestUpdate('_package');
	}

	#renderDocumentSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Content" description="Select the starting root content">
				<div slot="editor">
					<umb-input-document
						max="1"
						.value=${this._package.contentNodeId ?? undefined}
						@change=${this.#onContentChange}>
					</umb-input-document>
					<uui-checkbox
						label="Include child nodes"
						.checked=${this._package.contentLoadChildNodes ?? false}
						.disabled=${!this._package.contentNodeId}
						@change=${(e: UUIBooleanInputEvent) => (this._package!.contentLoadChildNodes = e.target.checked)}>
						Include child nodes
					</uui-checkbox>
				</div>
			</umb-property-layout>
		`;
	}

	#onMediaChange(event: { target: UmbInputMediaElement }) {
		if (!this._package) return;

		this._package.mediaIds = event.target.selection ?? [];

		if (this._package.mediaLoadChildNodes && !this._package.mediaIds.length) {
			this._package.mediaLoadChildNodes = false;
		}

		this.requestUpdate('_package');
	}

	#renderMediaSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Media">
				<div slot="editor">
					<umb-input-media
						multiple
						.selection=${this._package.mediaIds ?? []}
						@change=${this.#onMediaChange}></umb-input-media>
					<uui-checkbox
						label="Include child nodes"
						.checked=${this._package.mediaLoadChildNodes ?? false}
						.disabled=${!this._package.mediaIds?.length}
						@change=${(e: UUIBooleanInputEvent) => (this._package!.mediaLoadChildNodes = e.target.checked)}>
						Include child nodes
					</uui-checkbox>
				</div>
			</umb-property-layout>
		`;
	}

	#renderDocumentTypeSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Document Types">
				<div slot="editor">
					<umb-input-document-type
						.selection=${this._package.documentTypes ?? []}
						@change=${(e: CustomEvent) =>
							(this._package!.documentTypes = (e.target as UmbInputDocumentTypeElement).selection)}>
					</umb-input-document-type>
				</div>
			</umb-property-layout>
		`;
	}

	#renderMediaTypeSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Media Types">
				<div slot="editor">
					<umb-input-media-type
						.selection=${this._package.mediaTypes ?? []}
						@change=${(e: CustomEvent) =>
							(this._package!.mediaTypes = (e.target as UmbInputMediaTypeElement).selection)}>
					</umb-input-media-type>
				</div>
			</umb-property-layout>
		`;
	}

	#renderLanguageSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Languages">
				<div slot="editor">
					<umb-input-language
						.value="${this._package.languages?.join(',') ?? ''}"
						@change="${(e: CustomEvent) => {
							this._package!.languages = (e.target as UmbInputLanguageElement).selection;
						}}"></umb-input-language>
				</div>
			</umb-property-layout>
		`;
	}

	#onDictionaryChange(event: { target: UmbInputEntityElement }) {
		if (!this._package) return;

		this._package.dictionaryItems = event.target.selection ?? [];
		this.requestUpdate('_package');
	}

	#renderDictionarySection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Dictionary">
				<div slot="editor">
					<umb-input-entity
						.getIcon=${() => 'icon-book-alt'}
						.pickerContext=${UmbDictionaryPickerContext}
						.selection=${this._package.dictionaryItems ?? []}
						@change=${this.#onDictionaryChange}>
					</umb-input-entity>
				</div>
			</umb-property-layout>
		`;
	}

	#renderDataTypeSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Data Types">
				<div slot="editor">
					<umb-data-type-input
						.selection=${this._package.dataTypes}
						@change=${(e: CustomEvent) =>
							(this._package!.dataTypes = (e.target as UmbDataTypeInputElement).selection ?? [])}></umb-data-type-input>
				</div>
			</umb-property-layout>
		`;
	}

	#onTemplateChange(event: { target: UmbInputEntityElement }) {
		if (!this._package) return;

		this._package.templates = event.target.selection ?? [];
		this.requestUpdate('_package');
	}

	#renderTemplateSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Templates">
				<div slot="editor">
					<umb-input-entity
						.getIcon=${() => 'icon-newspaper'}
						.pickerContext=${UmbTemplatePickerContext}
						.selection=${this._package.templates ?? []}
						@change=${this.#onTemplateChange}>
					</umb-input-entity>
				</div>
			</umb-property-layout>
		`;
	}

	#onStylesheetsChange(event: { target: UmbInputEntityElement }) {
		if (!this._package) return;

		this._package.stylesheets =
			event.target.selection?.map((path) => this.#serverFilePathUniqueSerializer.toServerPath(path) as string) ?? [];
		this.requestUpdate('_package');
	}

	#renderStylesheetsSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Stylesheets">
				<div slot="editor">
					<umb-input-entity
						.getIcon=${() => 'icon-brush-alt'}
						.pickerContext=${UmbStylesheetPickerContext}
						.selection=${this._package.stylesheets.map((path) => this.#serverFilePathUniqueSerializer.toUnique(path)) ??
						[]}
						@change=${this.#onStylesheetsChange}>
					</umb-input-entity>
				</div>
			</umb-property-layout>
		`;
	}

	#onScriptsChange(event: { target: UmbInputEntityElement }) {
		if (!this._package) return;

		this._package.scripts =
			event.target.selection?.map((path) => this.#serverFilePathUniqueSerializer.toServerPath(path) as string) ?? [];
		this.requestUpdate('_package');
	}

	#renderScriptsSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Scripts">
				<div slot="editor">
					<umb-input-entity
						.getIcon=${() => 'icon-diploma'}
						.pickerContext=${UmbScriptPickerContext}
						.selection=${this._package.scripts.map((path) => this.#serverFilePathUniqueSerializer.toUnique(path)) ?? []}
						@change=${this.#onScriptsChange}>
					</umb-input-entity>
				</div>
			</umb-property-layout>
		`;
	}

	#onPartialViewsChange(event: { target: UmbInputEntityElement }) {
		if (!this._package) return;

		this._package.partialViews =
			event.target.selection?.map((path) => this.#serverFilePathUniqueSerializer.toServerPath(path) as string) ?? [];
		this.requestUpdate('_package');
	}

	#renderPartialViewSection() {
		if (!this._package) return nothing;
		return html`
			<umb-property-layout label="Partial Views">
				<div slot="editor">
					<umb-input-entity
						.getIcon=${() => 'icon-notepad'}
						.pickerContext=${UmbPartialViewPickerContext}
						.selection=${this._package.partialViews.map((path) =>
							this.#serverFilePathUniqueSerializer.toUnique(path),
						) ?? []}
						@change=${this.#onPartialViewsChange}>
					</umb-input-entity>
				</div>
			</umb-property-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#header {
				display: flex;
				gap: var(--uui-size-space-4);
				margin-right: var(--uui-size-layout-1);
				width: 100%;
			}

			uui-input {
				width: 100%;
			}

			uui-box {
				margin: var(--uui-size-layout-2);
			}

			uui-checkbox {
				margin-top: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbWorkspacePackageBuilderElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-package-builder': UmbWorkspacePackageBuilderElement;
	}
}
