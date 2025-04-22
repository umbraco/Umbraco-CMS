import type { UmbVariantDatasetWorkspaceContext } from '../../contexts/index.js';
import { UMB_WORKSPACE_SPLIT_VIEW_CONTEXT } from './workspace-split-view.context.js';
import { css, customElement, html, nothing, query, ref, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbDataPathVariantQuery, umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import { UMB_PROPERTY_DATASET_CONTEXT, isNameablePropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UUIInputElement, UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-workspace-split-view-variant-selector')
export class UmbWorkspaceSplitViewVariantSelectorElement<
	VariantOptionModelType extends
		UmbEntityVariantOptionModel<UmbEntityVariantModel> = UmbEntityVariantOptionModel<UmbEntityVariantModel>,
> extends UmbLitElement {
	@query('#popover')
	private _popoverElement?: UUIPopoverContainerElement;

	@state()
	private _variantOptions: Array<VariantOptionModelType> = [];

	@state()
	private _cultureVariantOptions: Array<VariantOptionModelType> = [];

	@state()
	_activeVariants: Array<UmbVariantId> = [];

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
	private _readOnlyCultures: Array<string | null> = [];

	@state()
	private _variesByCulture = false;

	@state()
	private _variesBySegment = false;

	@state()
	private _expandedVariants: Array<UmbVariantId> = [];

	@state()
	private _labelDefault = '';

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected _variantSorter = (a: VariantOptionModelType, b: VariantOptionModelType) => {
		return 0;
	};

	constructor() {
		super();

		this._labelDefault = this.localize.term('general_default');

		this.consumeContext(UMB_WORKSPACE_SPLIT_VIEW_CONTEXT, (instance) => {
			this.#splitViewContext = instance;

			const workspaceContext =
				this.#splitViewContext.getWorkspaceContext() as unknown as UmbVariantDatasetWorkspaceContext;
			if (!workspaceContext) throw new Error('Split View Workspace context not found');

			this.#observeVariants(workspaceContext);
			this.#observeActiveVariants(workspaceContext);
			this.#observeCurrentVariant();

			this.observe(
				workspaceContext.variesBySegment,
				(value) => (this._variesBySegment = value ?? false),
				'umbObserveVariesBySegment',
			);

			this.observe(
				workspaceContext.variesByCulture,
				(value) => (this._variesByCulture = value ?? false),
				'umbObserveVariesByCulture',
			);
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (instance) => {
			this.#datasetContext = instance;
			this.#observeDatasetContext();
			this.#observeCurrentVariant();
		});
	}

	async #observeVariants(workspaceContext: UmbVariantDatasetWorkspaceContext) {
		this.observe(
			workspaceContext.variantOptions,
			(variantOptions) => {
				this._variantOptions = (variantOptions as Array<VariantOptionModelType>).sort(this._variantSorter);
				this._cultureVariantOptions = this._variantOptions.filter((variant) => variant.segment === null);
				this.#setReadOnlyCultures(workspaceContext);
			},
			'_observeVariantOptions',
		);
	}

	async #observeActiveVariants(workspaceContext: UmbVariantDatasetWorkspaceContext) {
		this.observe(
			workspaceContext.splitView.activeVariantsInfo,
			(activeVariants) => {
				if (activeVariants) {
					this._activeVariants = activeVariants.map((variant) => UmbVariantId.Create(variant));
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
		const workspaceContext =
			this.#splitViewContext.getWorkspaceContext() as unknown as UmbVariantDatasetWorkspaceContext;
		if (!workspaceContext) return;

		this._variantId = this.#datasetContext.getVariantId();

		this.observe(
			workspaceContext.variantOptions,
			(options) => {
				const option = options.find(
					(option) => option.culture === this._variantId?.culture && option.segment === this._variantId?.segment,
				);
				this._activeVariant = option as VariantOptionModelType;
			},
			'umbObserveActiveVariant',
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

	#isVariantActive(variantId: UmbVariantId) {
		return this._activeVariants.find((activeVariantId) => activeVariantId.equal(variantId)) !== undefined;
	}

	#isCreateMode(variantOption: VariantOptionModelType, variantId: UmbVariantId) {
		return !variantOption.variant && !this.#isVariantActive(variantId);
	}

	#selectorIsEnabled() {
		// only varies by segment
		if (!this._variesByCulture && this._variesBySegment) {
			return this._cultureVariantOptions.length > 1 || this._variantOptions[0].variant?.state;
		}

		return this._variantOptions.length > 1;
	}

	#setReadOnlyCultures(workspaceContext: UmbVariantDatasetWorkspaceContext) {
		this._readOnlyCultures = this._variantOptions
			.filter((variant) => workspaceContext.readOnlyGuard.getIsPermittedForVariant(UmbVariantId.Create(variant)))
			.map((variant) => variant.culture);
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

		// If the active variant is a segment then we expend the culture variant when the selector is opened.
		if (this.#isSegmentVariantOption(this._activeVariant)) {
			const culture = this._cultureVariantOptions.find((variant) => {
				return variant.culture === this._activeVariant?.culture && variant.segment === null;
			});

			if (!culture) return;
			const variantId = UmbVariantId.Create(culture);
			this.#expandVariant(variantId);
		}
	}

	/**
	 * Focuses the input element after a short delay to ensure it is rendered.
	 * This works better than the {umbFocus()} directive, which does not work in this context.
	 * @param {Element} element – The element to focus.
	 */
	#focusInput(element?: Element) {
		if (!element) return;

		setTimeout(async () => {
			await this.updateComplete;
			(element as UUIInputElement)?.focus();
		}, 200);
	}

	#isReadOnlyCulture(culture: string | null) {
		return this._readOnlyCultures.includes(culture);
	}

	#isSegmentVariantOption(variantOption: VariantOptionModelType | undefined) {
		return variantOption?.segment !== null;
	}

	#onVariantExpandClick(event: PointerEvent, variantId: UmbVariantId) {
		event.stopPropagation();
		this.#toggleVariantExpansion(variantId);
	}

	#expandVariant(variantId: UmbVariantId) {
		if (this.#isVariantExpanded(variantId)) return;
		this._expandedVariants = [...this._expandedVariants, variantId];
	}

	#toggleVariantExpansion(variantId: UmbVariantId) {
		this._expandedVariants = this.#isVariantExpanded(variantId)
			? this._expandedVariants.filter((expandedVariant) => expandedVariant.equal(variantId) === false)
			: [...this._expandedVariants, variantId];
	}

	#isVariantExpanded(variantId: UmbVariantId) {
		return this._expandedVariants.find((expandedVariant) => expandedVariant.equal(variantId)) !== undefined;
	}

	#getSegmentVariantOptionsForCulture(
		variantOption: VariantOptionModelType,
		variantId: UmbVariantId,
	): Array<VariantOptionModelType> {
		const segmentVariants = this._variantOptions.filter(
			(variant) => variant.culture === variantId.culture && variant.segment !== null,
		);
		return variantOption.variant ? segmentVariants : [];
	}

	override render() {
		if (!this._variantId) return nothing;

		return html`
			<uui-input
				id="name-input"
				data-mark="input:entity-name"
				placeholder=${this.localize.term('placeholders_entername')}
				label=${this.localize.term('placeholders_entername')}
				.value=${this.#getNameValue()}
				@input=${this.#handleInput}
				required
				?readonly=${this.#isReadOnlyCulture(this._activeVariant?.culture ?? null) ||
				this.#isSegmentVariantOption(this._activeVariant)}
				${umbBindToValidation(this, `$.variants[${UmbDataPathVariantQuery(this._variantId)}].name`, this._name ?? '')}
				${ref(this.#focusInput)}>
				${this.#selectorIsEnabled()
					? html`
							<uui-button
								id="toggle"
								compact
								slot="append"
								popovertarget="popover"
								title=${this.#getVariantSpecInfo(this._activeVariant)}
								label="Select a variant">
								${this.#getVariantSpecInfo(this._activeVariant)}
								${this.#renderReadOnlyTag(this._activeVariant?.culture)}
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
					: html`<span id="read-only-tag" slot="append"> ${this.#renderReadOnlyTag(null)} </span>`}
			</uui-input>

			${this.#selectorIsEnabled()
				? html`
						<uui-popover-container id="popover" @beforetoggle=${this.#onPopoverToggle} placement="bottom-end">
							<div id="dropdown">
								<uui-scroll-container>
									${this._cultureVariantOptions.map((variant) => this.#renderCultureVariantOption(variant))}
								</uui-scroll-container>
							</div>
						</uui-popover-container>
					`
				: nothing}
		`;
	}

	#renderCultureVariantOption(variantOption: VariantOptionModelType) {
		const variantId = UmbVariantId.Create(variantOption);
		const notCreated = this.#isCreateMode(variantOption, variantId);
		const subVariantOptions = this.#getSegmentVariantOptionsForCulture(variantOption, variantId);
		return html`
			<div class="variant culture-variant ${this.#isVariantActive(variantId) ? 'selected' : ''}">
				${this._variesBySegment && this.#isCreated(variantOption) && subVariantOptions.length > 0
					? html`<div class="expand-area">${this.#renderExpandToggle(variantId)}</div>`
					: nothing}

				<button
					class="switch-button ${notCreated ? 'add-mode' : ''} ${this.#isReadOnlyCulture(variantId.culture)
						? 'readonly-mode'
						: ''}"
					@click=${() => this.#switchVariant(variantOption)}>
					${notCreated ? html`<uui-icon class="add-icon" name="icon-add"></uui-icon>` : nothing}
					<div class="variant-info">
						<div class="variant-name">
							${this.#getVariantDisplayName(variantOption)}${this.#renderReadOnlyTag(variantId.culture)}
						</div>
						<div class="variant-details">
							<span>${this._renderVariantDetails(variantOption)}</span>
						</div>
					</div>
					<div class="specs-info">${this.#getVariantSpecInfo(variantOption)}</div>
				</button>
				${this.#renderSplitViewButton(variantOption)}
			</div>
			${this.#isVariantExpanded(variantId)
				? html` ${subVariantOptions.map((option) => this.#renderSegmentVariantOption(option))} `
				: nothing}
		`;
	}

	#isCreated(variantOption: VariantOptionModelType) {
		return (
			variantOption.variant?.state &&
			variantOption.variant?.state !== ('NotCreated' as DocumentVariantStateModel.NOT_CREATED)
		);
	}

	#renderExpandToggle(variantId: UmbVariantId) {
		return html`
			<uui-button @click=${(event: PointerEvent) => this.#onVariantExpandClick(event, variantId)} compact>
				<uui-symbol-expand .open=${this.#isVariantExpanded(variantId)}></uui-symbol-expand>
			</uui-button>
		`;
	}

	#renderSegmentVariantOption(variantOption: VariantOptionModelType) {
		const variantId = UmbVariantId.Create(variantOption);
		const notCreated = this.#isCreateMode(variantOption, variantId);

		return html`
			<div class="variant segment-variant ${this.#isVariantActive(variantId) ? 'selected' : ''}">
				${notCreated ? nothing : html`<div class="expand-area"></div>`}
				<button
					class="switch-button ${notCreated ? 'add-mode' : ''} ${this.#isReadOnlyCulture(variantId.culture)
						? 'readonly-mode'
						: ''}"
					@click=${() => this.#switchVariant(variantOption)}>
					${notCreated ? html`<uui-icon class="add-icon" name="icon-add"></uui-icon>` : nothing}
					<div class="variant-info">
						<div class="variant-name">
							${this.#getVariantDisplayName(variantOption)}${this.#renderReadOnlyTag(variantId.culture)}
						</div>
						<div class="variant-details">
							<span>${this._renderVariantDetails(variantOption)}</span>
						</div>
					</div>
					<div class="specs-info">${this.#getVariantSpecInfo(variantOption)}</div>
				</button>
				${this.#renderSplitViewButton(variantOption)}
			</div>
		`;
	}

	#getNameValue() {
		// It is currently not possible to edit the name of a segment variant option. We render the name of the segment instead and set the input to readonly.
		const segmentName =
			this.#isSegmentVariantOption(this._activeVariant) && this._activeVariant?.segmentInfo?.name
				? this._activeVariant.segmentInfo.name
				: '';
		return segmentName !== '' ? segmentName : (this._name ?? '');
	}

	#getVariantDisplayName(variantOption: VariantOptionModelType) {
		if (this.#isSegmentVariantOption(variantOption)) {
			return variantOption?.segmentInfo?.name ?? this._labelDefault;
		}

		return variantOption.variant?.name ?? variantOption.language.name;
	}

	#getVariantSpecInfo(variantOption: VariantOptionModelType | undefined) {
		if (!variantOption) {
			return '';
		}

		// If we vary by culture and segment, we show both
		if (this._variesByCulture && this._variesBySegment) {
			return variantOption.segmentInfo
				? `${variantOption.language.name} - ${variantOption.segmentInfo.name}`
				: variantOption.language.name || this._labelDefault;
		}

		// If we vary by segment only, we only show the segment and show "Default" for the language
		if (!this._variesByCulture && this._variesBySegment) {
			return variantOption?.segmentInfo?.name ?? this._labelDefault;
		}

		return variantOption.language.name;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected _renderVariantDetails(variantOption: VariantOptionModelType) {
		return html``;
	}

	#renderReadOnlyTag(culture?: string | null) {
		if (culture === undefined) return nothing;
		return this.#isReadOnlyCulture(culture)
			? html`<uui-tag look="secondary">${this.localize.term('general_readOnly')}</uui-tag>`
			: nothing;
	}

	#renderSplitViewButton(variant: VariantOptionModelType) {
		const variantId = UmbVariantId.Create(variant);
		return html`
			${this.#isVariantActive(variantId)
				? nothing
				: html`
						<uui-button
							style="background-color: var(--uui-color-surface)"
							label="Open Split view for ${variant.language.name}"
							class="split-view"
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

			#toggle {
				white-space: nowrap;
			}

			#popover {
				translate: 1px; /* Fixes tiny alignment issue caused by border */
			}

			#dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: auto;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
			}

			#variant-close {
				white-space: nowrap;
			}

			#read-only-tag {
				white-space: nowrap;
				display: flex;
				align-items: center;
				justify-content: center;
			}

			uui-scroll-container {
				max-height: 50dvh;
			}

			.variant {
				position: relative;
				display: flex;
				border-top: 1px solid var(--uui-color-divider-standalone);
			}

			.expand-area {
				position: relative;
				display: block;
				width: var(--uui-size-12);
				align-items: center;
				justify-content: center;
			}

			.expand-area uui-button {
				height: 100%;
				width: 100%;
			}

			uui-symbol-expand {
				background: none;
			}

			.variant:hover > .split-view {
				display: flex;
			}

			.variant:nth-last-of-type(1) {
				margin-bottom: 0;
			}

			.variant.selected:before {
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

			.switch-button {
				display: flex;
				align-items: center;
				border: none;
				background: transparent;
				color: var(--uui-color-current-contrast);
				padding: var(--uui-size-space-2) var(--uui-size-space-6);
				font-weight: bold;
				width: 100%;
				text-align: left;
				font-size: 14px;
				cursor: pointer;
			}

			.expand-area + .switch-button {
				padding-left: var(--uui-size-space-3);
			}

			.segment-variant > .switch-button {
				padding-left: var(--uui-size-space-6);
			}

			.switch-button:hover {
				background: var(--uui-palette-sand);
				color: var(--uui-palette-space-cadet-light);
			}
			.switch-button .variant-info {
				flex-grow: 1;
			}

			.switch-button .variant-details {
				color: var(--uui-color-text-alt);
				font-size: var(--uui-type-small-size);
				font-weight: normal;
			}
			.switch-button .variant-details {
				color: var(--uui-color-text-alt);
				font-size: var(--uui-type-small-size);
				font-weight: normal;
			}
			.switch-button.add-mode .variant-details {
				color: var(--uui-palette-dusty-grey-dark);
			}

			.switch-button .specs-info {
				color: var(--uui-color-text-alt);
				font-size: var(--uui-type-small-size);
				font-weight: normal;
			}
			.switch-button.add-mode .specs-info {
				color: var(--uui-palette-dusty-grey-dark);
			}

			.switch-button i {
				font-weight: normal;
			}

			.switch-button.add-mode {
				position: relative;
				color: var(--uui-palette-dusty-grey-dark);
			}

			.switch-button.add-mode:after {
				border: 1px dashed var(--uui-color-divider-standalone);
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

			.switch-button .variant-name {
				margin-bottom: var(--uui-size-space-1);
			}

			.switch-button.readonly-mode .variant-name {
				margin-bottom: calc(var(--uui-size-space-1) * -1);
			}

			.add-icon {
				font-size: var(--uui-type-small-size);
				margin-right: 21px;
			}

			.split-view {
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
		'umb-workspace-split-view-variant-selector': UmbWorkspaceSplitViewVariantSelectorElement;
	}
}
