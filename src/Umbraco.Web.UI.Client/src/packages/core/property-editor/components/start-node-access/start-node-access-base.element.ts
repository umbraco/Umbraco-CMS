import type { UmbPropertyEditorConfigCollection } from '../../config/index.js';
import type { UmbPropertyEditorUiElement } from '../../extensions/property-editor-ui-element.interface.js';
import { css, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType, UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

/**
 * The value of a start-node-access property editor: either unrestricted ("root") access, or access
 * limited to the given set of start nodes.
 */
export type UmbStartNodeAccessValue = {
	rootAccess: boolean;
	startNodes: Array<UmbReferenceByUnique>;
};

/**
 * Abstract base for a property editor UI that offers either unrestricted ("root") access or access
 * limited to a set of picked start nodes, toggled by a single boolean.
 *
 * Subclasses only need to implement `renderPicker()`, rendering the entity-specific input bound to
 * `_selection`/`_onPick` and forwarding `readonly`/`_min`/`_max`.
 */
export abstract class UmbStartNodeAccessPropertyEditorUiElementBase
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	/**
	 * Sets the input to readonly mode, meaning the value cannot be changed but the toggle and picker can still be read.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * The current start-node-access value.
	 * @type {UmbStartNodeAccessValue}
	 */
	@property({ attribute: false })
	public set value(value: UmbStartNodeAccessValue | undefined) {
		this._rootAccess = value?.rootAccess ?? false;
		this._startNodes = value?.startNodes ?? [];
	}
	public get value(): UmbStartNodeAccessValue {
		return { rootAccess: this._rootAccess, startNodes: this._startNodes };
	}

	/**
	 * The property editor config. Reads the `rootAccessLabel` alias for the root-access toggle label, and the
	 * `validationLimit` alias (`{min, max}`) forwarded to the picker as selection limits.
	 * @type {UmbPropertyEditorConfigCollection}
	 */
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._rootAccessLabel = config.getValueByAlias<string>('rootAccessLabel') ?? '';

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;
	}

	@state()
	protected _rootAccess = false;

	@state()
	protected _startNodes: Array<UmbReferenceByUnique> = [];

	@state()
	private _rootAccessLabel = '';

	@state()
	protected _min = 0;

	@state()
	protected _max = Infinity;

	protected get _selection(): Array<string> {
		return this._startNodes.map((reference) => reference.unique);
	}

	/**
	 * Renders the entity-specific picker, shown whenever root access is off. Implementations should
	 * bind their selection to `_selection`, forward `readonly`/`_min`/`_max`, and call `_onPick` on change.
	 */
	protected abstract renderPicker(): unknown;

	protected _onPick(selection: Array<string>) {
		this._rootAccess = false;
		this._startNodes = selection.map((unique) => ({ unique }));
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onToggle(event: UUIBooleanInputEvent) {
		this._rootAccess = event.target.checked;
		this._startNodes = [];
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-toggle
				.checked=${this._rootAccess}
				.label=${this._rootAccessLabel}
				?readonly=${this.readonly}
				@change=${this.#onToggle}></uui-toggle>
			${when(this._rootAccess === false, () => this.renderPicker())}
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
		`,
	];
}
