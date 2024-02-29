import { UmbDocumentTypeCompositionRepository } from '../../repository/index.js';
import type {
	UmbCompositionPickerModalData,
	UmbCompositionPickerModalValue,
} from './composition-picker-modal.token.js';
import { css, html, customElement, state, repeat, nothing, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type {
	UmbDocumentTypeCompositionCompatibleModel,
	UmbDocumentTypeCompositionReferenceModel,
} from '@umbraco-cms/backoffice/document-type';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';

interface CompatibleCompositions {
	path: string;
	compositions: Array<UmbDocumentTypeCompositionCompatibleModel>;
}

@customElement('umb-composition-picker-modal')
export class UmbCompositionPickerModalElement extends UmbModalBaseElement<
	UmbCompositionPickerModalData,
	UmbCompositionPickerModalValue
> {
	#compositionRepository = new UmbDocumentTypeCompositionRepository(this);
	#unique?: string;

	@state()
	private _references: Array<UmbDocumentTypeCompositionReferenceModel> = [];

	@state()
	private _compatibleCompositions?: Array<CompatibleCompositions>;

	@state()
	private _selection: Array<string> = [];

	connectedCallback() {
		super.connectedCallback();

		this._selection = this.data?.selection ?? [];
		this.modalContext?.setValue({ selection: this._selection });

		this.#requestReference();
	}

	async #requestReference() {
		this.#unique = this.data?.unique;
		if (!this.#unique) return;

		const { data } = await this.#compositionRepository.getReferences(this.#unique);

		this._references = data ?? [];

		if (!this._references.length) {
			this.#requestAvailableCompositions();
		}
	}

	async #requestAvailableCompositions() {
		if (!this.#unique) return;

		const isElement = this.data?.isElement;
		const currentPropertyAliases = this.data?.currentPropertyAliases;

		const { data } = await this.#compositionRepository.availableCompositions({
			unique: this.#unique,
			isElement: isElement ?? false,
			currentCompositeUniques: this._selection,
			currentPropertyAliases: currentPropertyAliases ?? [],
		});

		if (!data) return;

		const folders = Array.from(new Set(data.map((c) => '/' + c.folderPath.join('/'))));
		this._compatibleCompositions = folders.map((path) => ({
			path,
			compositions: data.filter((c) => '/' + c.folderPath.join('/') === path),
		}));
	}

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('contentTypeEditor_compositions')}">
				${this._references.length ? this.#renderHasReference() : this.#renderAvailableCompositions()}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					${!this._references.length
						? html`<uui-button
								label=${this.localize.term('general_submit')}
								look="primary"
								color="positive"
								@click=${this._submitModal}></uui-button>`
						: nothing}
				</div>
			</umb-body-layout>
		`;
	}

	#renderHasReference() {
		return html` <umb-localize key="contentTypeEditor_compositionInUse">
				This Content Type is used in a composition, and therefore cannot be composed itself.
			</umb-localize>
			<h4>
				<umb-localize key="contentTypeEditor_compositionUsageHeading">Where is this composition used?</umb-localize>
			</h4>
			<umb-localize key="contentTypeEditor_compositionUsageSpecification">
				This composition is currently used in the composition of the following Content Types:
			</umb-localize>
			<div class="reference-list">
				${repeat(
					this._references,
					(item) => item.unique,
					(item) => {
						const [icon, color] = item.icon ? item.icon.split(' ') : [];
						const variable = extractUmbColorVariable(color?.replace('color-', ''));
						console.log(icon, color);
						return html`<uui-ref-node-document-type
							href=${'/section/settings/workspace/document-type/edit/' + item.unique}
							name=${item.name}>
							<uui-icon
								slot="icon"
								name=${icon}
								style=${ifDefined(variable ? `--uui-icon-color:var(${variable})` : undefined)}></uui-icon>
						</uui-ref-node-document-type>`;
					},
				)}
			</div>`;
	}

	#renderAvailableCompositions() {
		if (this._compatibleCompositions) {
			return html`<umb-localize key="contentTypeEditor_compositionsDescription">
					Inherit tabs and properties from an existing Document Type. New tabs will be<br />added to the current
					Document Type or merged if a tab with an identical name exists.<br />
				</umb-localize>
				<!--- TODO: Implement search functionality
				<uui-input id="search" placeholder=this.localize.term('placeholders_filter')>
					<uui-icon name="icon-search" slot="prepend"></uui-icon>
				</uui-input> -->
				<div class="compositions-list">
					${repeat(
						this._compatibleCompositions,
						(folder) => folder.path,
						(folder) =>
							html`${this._compatibleCompositions!.length > 1
								? html`<strong><uui-icon name="icon-folder"></uui-icon>${folder.path}</strong>`
								: nothing}
							${this.#renderCompositionsItems(folder.compositions)}`,
					)}
				</div>`;
		} else {
			return html`<umb-localize key="contentTypeEditor_noAvailableCompositions">
				There are no Content Types available to use as a composition
			</umb-localize>`;
		}
	}

	#onSelectionAdd(unique: string) {
		this._selection = [...this._selection, unique];
		this.modalContext?.setValue({ selection: this._selection });
	}

	#onSelectionRemove(unique: string) {
		this._selection = this._selection.filter((s) => s !== unique);
		this.modalContext?.setValue({ selection: this._selection });
	}

	#renderCompositionsItems(compositionsList: Array<UmbDocumentTypeCompositionCompatibleModel>) {
		return repeat(
			compositionsList,
			(compositions) => compositions.unique,
			(compositions) =>
				html`<uui-menu-item
					label=${compositions.name}
					selectable
					@selected=${() => this.#onSelectionAdd(compositions.unique)}
					@deselected=${() => this.#onSelectionRemove(compositions.unique)}
					?selected=${this._selection.find((unique) => unique === compositions.unique)}>
					<uui-icon name=${compositions.icon} slot="icon"></uui-icon>
				</uui-menu-item>`,
		);
	}

	static styles = [
		css`
			uui-input {
				margin: var(--uui-size-6) 0;
				display: flex;
				align-items: center;
			}

			#search uui-icon {
				padding-left: var(--uui-size-3);
			}

			.reference-list {
				margin-block: var(--uui-size-3);
				display: grid;
				gap: var(--uui-size-1);
			}

			.compositions-list strong {
				display: flex;
				align-items: center;
				gap: var(--uui-size-3);
			}
		`,
	];
}

export default UmbCompositionPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-composition-picker-modal': UmbCompositionPickerModalElement;
	}
}
