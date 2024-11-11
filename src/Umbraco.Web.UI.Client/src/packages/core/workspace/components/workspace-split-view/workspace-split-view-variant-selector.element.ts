import type { ActiveVariant } from '../../controllers/index.js';
import { UMB_WORKSPACE_SPLIT_VIEW_CONTEXT } from './workspace-split-view.context.js';
import {
	type UUIInputElement,
	UUIInputEvent,
	type UUIPopoverContainerElement,
} from '@umbraco-cms/backoffice/external/uui';
import {
	css,
	html,
	nothing,
	customElement,
	state,
	query,
	ifDefined,
	type TemplateResult,
} from '@umbraco-cms/backoffice/external/lit';
import {
	UmbVariantId,
	type UmbEntityVariantModel,
	type UmbEntityVariantOptionModel,
} from '@umbraco-cms/backoffice/variant';
import { UMB_PROPERTY_DATASET_CONTEXT, isNameablePropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbVariantState } from '@umbraco-cms/backoffice/utils';
import { UmbDataPathVariantQuery, umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import type { UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';

const elementName = 'umb-workspace-split-view-variant-selector';
@customElement(elementName)
export class UmbWorkspaceSplitViewVariantSelectorElement<
	VariantOptionModelType extends
		UmbEntityVariantOptionModel<UmbEntityVariantModel> = UmbEntityVariantOptionModel<UmbEntityVariantModel>,
> extends UmbLitElement {
	@query('#variant-selector-popover')
	private _popoverElement?: UUIPopoverContainerElement;

	@state()
	private _variantOptions: Array<VariantOptionModelType> = [];

	@state()
	private _readOnlyStates: Array<UmbVariantState> = [];

	@state()
	_activeVariants: Array<ActiveVariant> = [];

	@state()
	_activeVariantsCultures: string[] = [];

	#splitViewContext?: typeof UMB_WORKSPACE_SPLIT_VIEW_CONTEXT.TYPE;
	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	@state()
	private _name?: string;

	@state()
	private _activeVariant?: VariantOptionModelType;

	@state()
	private _variantId?: UmbVariantId;

	@state()
	private _variantSelectorOpen = false;

	@state()
	private _readOnlyCultures: string[] = [];

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected _variantSorter = (a: VariantOptionModelType, b: VariantOptionModelType) => {
		return 0;
	};

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_SPLIT_VIEW_CONTEXT, (instance) => {
			this.#splitViewContext = instance;

			const workspaceContext = this.#splitViewContext.getWorkspaceContext() as unknown as UmbContentWorkspaceContext;
			if (!workspaceContext) throw new Error('Split View Workspace context not found');

			this.#observeVariants(workspaceContext);
			this.#observeActiveVariants(workspaceContext);
			this.#observeReadOnlyStates(workspaceContext);
			this.#observeCurrentVariant();
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (instance) => {
			this.#datasetContext = instance;
			this.#observeDatasetContext();
			this.#observeCurrentVariant();
		});
	}

	async #observeVariants(workspaceContext: UmbContentWorkspaceContext) {
		this.observe(
			workspaceContext.variantOptions,
			(variantOptions) => {
				this._variantOptions = (variantOptions as Array<VariantOptionModelType>).sort(this._variantSorter);
				this.#setReadOnlyCultures();
			},
			'_observeVariantOptions',
		);
	}

	async #observeReadOnlyStates(workspaceContext: UmbContentWorkspaceContext) {
		this.observe(
			workspaceContext.readOnlyState.states,
			(states) => {
				this._readOnlyStates = states;
				this.#setReadOnlyCultures();
			},
			'umbObserveReadOnlyStates',
		);
	}

	async #observeActiveVariants(workspaceContext: UmbContentWorkspaceContext) {
		this.observe(
			workspaceContext.splitView.activeVariantsInfo,
			(activeVariants) => {
				if (activeVariants) {
					this._activeVariants = activeVariants;
					this._activeVariantsCultures = this._activeVariants.map((el) => el.culture ?? '') ?? [];
				}
			},
			'_observeActiveVariants',
		);
	}

	async #observeDatasetContext() {
		if (!this.#datasetContext) return;
		this.observe(
			this.#datasetContext.name,
			(name) => {
				this._name = name;
			},
			'_name',
		);
	}

	async #observeCurrentVariant() {
		if (!this.#datasetContext || !this.#splitViewContext) return;
		const workspaceContext = this.#splitViewContext.getWorkspaceContext() as unknown as UmbContentWorkspaceContext;
		if (!workspaceContext) return;

		this._variantId = this.#datasetContext.getVariantId();

		this.observe(
			workspaceContext.variantOptions,
			(options) => {
				const option = options.find((option) => option.language.unique === this._variantId?.culture);
				this._activeVariant = option as VariantOptionModelType;
			},
			'_currentLanguage',
		);
	}

	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (
				typeof target?.value === 'string' &&
				this.#datasetContext &&
				isNameablePropertyDatasetContext(this.#datasetContext)
			) {
				this.#datasetContext.setName(target.value);
			}
		}
	}

	#switchVariant(variant: VariantOptionModelType) {
		this.#splitViewContext?.switchVariant(UmbVariantId.Create(variant));
	}

	#openSplitView(variant: VariantOptionModelType) {
		this.#splitViewContext?.openSplitView(UmbVariantId.Create(variant));
	}

	#closeSplitView() {
		this.#splitViewContext?.closeSplitView();
	}

	#isVariantActive(culture: string | null) {
		return culture !== null ? this._activeVariantsCultures.includes(culture) : true;
	}

	#isCreateMode(variantOption: VariantOptionModelType) {
		return !variantOption.variant && !this.#isVariantActive(variantOption.culture);
	}

	#hasVariants() {
		return this._variantOptions?.length > 1;
	}

	#setReadOnlyCultures() {
		this._readOnlyCultures = this._variantOptions
			.filter((variant) => this._readOnlyStates.some((state) => state.variantId.compare(variant)))
			.map((variant) => variant.culture)
			.filter((item) => item !== null) as string[];
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._variantSelectorOpen = event.newState === 'open';

		if (!this._popoverElement) return;

		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		const isOpen = event.newState === 'open';
		if (!isOpen) return;

		const host = this.getBoundingClientRect();
		// TODO: Ideally this is kept updated while open, but for now we just set it once:
		this._popoverElement.style.width = `${host.width}px`;
	}

	override render() {
		return this._variantId
			? html`
			<uui-input
				id="name-input"
				label=${this.localize.term('placeholders_entername')}
				.value=${this._name ?? ''}
				@input=${this.#handleInput}
				required
				?readonly=${this.#isReadOnly(this._activeVariant?.culture ?? null)}
				${umbBindToValidation(this, `$.variants[${UmbDataPathVariantQuery(this._variantId)}].name`, this._name ?? '')}
				${umbFocus()}
			>
				${
					this.#hasVariants()
						? html`
								<uui-button
									id="variant-selector-toggle"
									compact
									slot="append"
									popovertarget="variant-selector-popover"
									title=${ifDefined(this._activeVariant?.language.name)}
									label="Select a variant">
									${this._activeVariant?.language.name} ${this.#renderReadOnlyTag(this._activeVariant?.culture)}
									<uui-symbol-expand .open=${this._variantSelectorOpen}></uui-symbol-expand>
								</uui-button>
								${this._activeVariants.length > 1
									? html`
											<uui-button slot="append" compact id="variant-close" @click=${this.#closeSplitView}>
												<uui-icon name="remove"></uui-icon>
											</uui-button>
										`
									: ''}
							`
						: nothing
				}
			</uui-input>

			${
				this.#hasVariants()
					? html`
							<uui-popover-container
								id="variant-selector-popover"
								@beforetoggle=${this.#onPopoverToggle}
								placement="bottom-end">
								<div id="variant-selector-dropdown">
									<uui-scroll-container>
										<ul>
											${this._variantOptions.map((variant) => this.#renderListItem(variant))}
										</ul>
									</uui-scroll-container>
								</div>
							</uui-popover-container>
						`
					: nothing
			}
		</div>
		`
			: nothing;
	}

	#renderListItem(variantOption: VariantOptionModelType) {
		return html`
			<li class="${this.#isVariantActive(variantOption.culture) ? 'selected' : ''}">
				<button
					class="variant-selector-switch-button ${this.#isCreateMode(variantOption)
						? 'add-mode'
						: ''} ${this.#isReadOnly(variantOption.culture) ? 'readonly-mode' : ''}"
					@click=${() => this.#switchVariant(variantOption)}>
					${this.#isCreateMode(variantOption) ? html`<uui-icon class="add-icon" name="icon-add"></uui-icon>` : nothing}
					<div class="variant-info">
						<div class="variant-name">
							${variantOption.variant?.name ?? variantOption.language.name}
							${this.#renderReadOnlyTag(variantOption.culture)}
						</div>
						<div class="variant-details">
							<span>${this._renderVariantDetails(variantOption)}</span>
							<span
								>${variantOption.language.isDefault
									? html`<span> - ${this.localize.term('general_default')}</span>`
									: nothing}</span
							>
						</div>
					</div>
					<div class="specs-info">${variantOption.language.name}</div>
				</button>
				${this.#renderSplitViewButton(variantOption)}
			</li>
		`;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected _renderVariantDetails(variantOption: VariantOptionModelType): TemplateResult {
		return html``;
	}

	#isReadOnly(culture: string | null) {
		if (!culture) return false;
		return this._readOnlyCultures.includes(culture);
	}

	#renderReadOnlyTag(culture?: string | null) {
		if (!culture) return nothing;
		return this.#isReadOnly(culture)
			? html`<uui-tag look="secondary">${this.localize.term('general_readOnly')}</uui-tag>`
			: nothing;
	}

	#renderSplitViewButton(variant: VariantOptionModelType) {
		return html`
			${this.#isVariantActive(variant.culture)
				? nothing
				: html`
						<uui-button
							style="background-color: var(--uui-color-surface)"
							label="Open Split view for ${variant.language.name}"
							class="variant-selector-split-view"
							@click=${() => this.#openSplitView(variant)}>
							Open in Split view
						</uui-button>
					`}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#name-input {
				width: 100%;
			}

			#variant-selector-toggle {
				white-space: nowrap;
			}

			#variant-selector-popover {
				translate: 1px; /* Fixes tiny alignment issue caused by border */
			}

			#variant-selector-dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
			}

			#variant-close {
				white-space: nowrap;
			}

			ul {
				list-style-type: none;
				padding: 0;
				margin: 0;
			}

			li {
				position: relative;
				margin-bottom: 1px;
			}

			li:hover .variant-selector-split-view {
				display: flex;
			}

			li:nth-last-of-type(1) {
				margin-bottom: 0;
			}

			li.selected:before {
				background-color: var(--uui-color-current);
				border-radius: 0 4px 4px 0;
				bottom: 8px;
				content: '';
				left: 0;
				pointer-events: none;
				position: absolute;
				top: 8px;
				width: 4px;
				z-index: 1;
			}

			.variant-selector-switch-button {
				display: flex;
				align-items: center;
				border: none;
				background: transparent;
				color: var(--uui-color-current-contrast);
				padding: 6px 20px;
				font-weight: bold;
				width: 100%;
				text-align: left;
				font-size: 14px;
				cursor: pointer;
				border-bottom: 1px solid var(--uui-color-divider-standalone);
			}

			.variant-selector-switch-button:hover {
				background: var(--uui-palette-sand);
				color: var(--uui-palette-space-cadet-light);
			}
			.variant-selector-switch-button .variant-info {
				flex-grow: 1;
			}

			.variant-selector-switch-button .variant-details {
				color: var(--uui-color-text-alt);
				font-size: 12px;
				font-weight: normal;
			}
			.variant-selector-switch-button .variant-details {
				color: var(--uui-color-text-alt);
				font-size: 12px;
				font-weight: normal;
			}
			.variant-selector-switch-button.add-mode .variant-details {
				color: var(--uui-palette-dusty-grey-dark);
			}

			.variant-selector-switch-button .specs-info {
				color: var(--uui-color-text-alt);
				font-size: 12px;
				font-weight: normal;
			}
			.variant-selector-switch-button.add-mode .specs-info {
				color: var(--uui-palette-dusty-grey-dark);
			}

			.variant-selector-switch-button i {
				font-weight: normal;
			}

			.variant-selector-switch-button.add-mode {
				position: relative;
				color: var(--uui-palette-dusty-grey-dark);
			}

			.variant-selector-switch-button.add-mode:after {
				border: 2px dashed var(--uui-color-divider-standalone);
				bottom: 0;
				content: '';
				left: 0;
				margin: 2px;
				pointer-events: none;
				position: absolute;
				right: 0;
				top: 0;
				z-index: 1;
			}

			.variant-selector-switch-button .variant-name {
				margin-bottom: var(--uui-size-space-1);
			}

			.variant-selector-switch-button.readonly-mode .variant-name {
				margin-bottom: calc(var(--uui-size-space-1) * -1);
			}

			.add-icon {
				font-size: 12px;
				margin-right: 12px;
			}

			.variant-selector-split-view {
				position: absolute;
				top: 0;
				right: 0;
				bottom: 1px;
				display: none;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbWorkspaceSplitViewVariantSelectorElement;
	}
}
