import { getAddSectionSnippet, getRenderBodySnippet, getRenderSectionSnippet } from '../../utils/index.js';
import { TemplatingSectionType } from '../../types.js';
import type {
	UmbTemplatingSectionPickerModalData,
	UmbTemplatingSectionPickerModalValue,
} from './templating-section-picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIBooleanInputElement, UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-templating-section-picker-modal')
export class UmbTemplatingSectionPickerModalElement extends UmbModalBaseElement<
	UmbTemplatingSectionPickerModalData,
	UmbTemplatingSectionPickerModalValue
> {
	@query('#render-named-section-name')
	private _renderNamedSectionNameInput?: UUIInputElement;

	@query('#define-named-section-name')
	private _defineNamedSectionNameInput?: UUIInputElement;

	@query('#render-named-section-is-mandatory')
	private _renderNamedSectionIsMandatoryCheckbox?: UUIBooleanInputElement;

	@state()
	private _pickedSection?: TemplatingSectionType;

	#close() {
		this.modalContext?.reject();
	}


	#submit() {
		switch (this._pickedSection) {
			case TemplatingSectionType.renderChildTemplate:
				this.value = { value: getRenderBodySnippet() };
				break;
			case TemplatingSectionType.renderANamedSection:
				this.value = {
					value: getRenderSectionSnippet(
						this._renderNamedSectionNameInput?.value as string,
						this._renderNamedSectionIsMandatoryCheckbox?.checked ?? false,
					),
				};
				break;
			case TemplatingSectionType.defineANamedSection:
				this.value = { value: getAddSectionSnippet(this._defineNamedSectionNameInput?.value as string) };
				break;
		}
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('template_insert')}>
				<uui-box>
					<div id="main">
						${this.#renderRenderChildTemplate()} ${this.#renderRenderANamedSection()}
						${this.#renderDefineANamedSection()}
					</div>
				</uui-box>

				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" label=${this.localize.term('general_close')}></uui-button>
					<uui-button
						@click=${this.#submit}
						look="primary"
						color="positive"
						?disabled=${this._pickedSection === undefined}
						label=${this.localize.term('general_submit')}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderRenderChildTemplate() {
		return html`<uui-card
			selectable
			selectOnly
			.selected=${this._pickedSection === TemplatingSectionType.renderChildTemplate}
			label=${this.localize.term('template_renderBody')}
			@selected=${() => this._pickedSection = TemplatingSectionType.renderChildTemplate}
  			@deselected=${() => this._pickedSection = undefined}>
			<h3><umb-localize key="template_renderBody">Render Child Template</umb-localize></h3>
			<p>
				<umb-localize key="template_renderBodyDesc">
					Renders the contents of a child template, by inserting a <code>@RenderBody()</code> placeholder.
				</umb-localize>
			</p>
		</uui-card>`;
	}

	#renderRenderANamedSection() {
		return html`<uui-card
			selectable
			selectOnly
			.selected=${this._pickedSection === TemplatingSectionType.renderANamedSection}
			label=${this.localize.term('template_renderSection')}
			@selected=${() => this._pickedSection = TemplatingSectionType.renderANamedSection}
  			@deselected=${() => this._pickedSection = undefined}>
			<h3><umb-localize key="template_renderSection">Render a named section</umb-localize></h3>
			<p>
				<umb-localize key="template_renderSectionDesc">
					Renders a named area of a child template, by inserting a
					<code>@RenderSection(name)</code> placeholder. This renders an area of a child template which is wrapped in a
					corresponding <code>@section [name]{ ... }</code> definition.
				</umb-localize>
			</p>
			${this._pickedSection === TemplatingSectionType.renderANamedSection
				? html`<div class="section">
						<uui-label for="render-named-section-name" required>
							<umb-localize key="template_sectionName">Section Name</umb-localize>
						</uui-label>
						<uui-input ${umbFocus()} id="render-named-section-name" label=${this.localize.term('template_sectionName')}></uui-input>
						<uui-checkbox
							id="render-named-section-is-mandatory"
							label=${this.localize.term('template_sectionMandatory')}
							@click=${(e: Event) => e.stopPropagation()}
							@keydown=${(e: KeyboardEvent) => {
        					if (e.key === 'Enter') {
            				(e.target as HTMLElement).click();}}}></uui-checkbox>
						<small>
							<umb-localize key="template_sectionMandatoryDesc">
								If mandatory, the child template must contain a <code>@section</code> definition, otherwise an error is
								shown.
							</umb-localize>
						</small>
					</div>`
				: ''}
		</uui-card>`;
	}

	#renderDefineANamedSection() {
		return html`<uui-card
			selectable
			selectOnly
			.selected=${this._pickedSection === TemplatingSectionType.defineANamedSection}
			label=${this.localize.term('template_defineSection')}
			@selected=${() => this._pickedSection = TemplatingSectionType.defineANamedSection}
  			@deselected=${() => this._pickedSection = undefined}>
			<h3><umb-localize key="template_defineSection">Define a named section</umb-localize></h3>
			<p>
				<umb-localize key="template_defineSectionDesc">
					Defines a part of your template as a named section by wrapping it in <code>@section { ... }</code>. This can
					be rendered in a specific area of the parent of this template, by using <code>@RenderSection</code>.
				</umb-localize>
			</p>
			${this._pickedSection === TemplatingSectionType.defineANamedSection
				? html`<div class="section">
						<uui-label for="define-named-section-name" required>
							<umb-localize key="template_sectionName">Section Name</umb-localize>
						</uui-label>
						<uui-input ${umbFocus()} id="define-named-section-name" label=${this.localize.term('template_sectionName')}></uui-input>
					</div>`
				: ''}
		</uui-card>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#main {
				display: grid;
				grid-gap: var(--uui-size-space-5);
			}

			.section {
				display: grid;
			}

			uui-card {
				text-align: left;
				display: block;
				padding: var(--uui-size-space-4);
			}

			uui-input,
			small {
				margin-block: var(--uui-size-space-2) var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbTemplatingSectionPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-section-picker-modal': UmbTemplatingSectionPickerModalElement;
	}
}