import { LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
/**
 * This component is used to display a single slice of a donut chart. It only makes sense insice the donut chart
 *
 * @export
 * @class UmbDonutSliceElement
 * @fires slice-update - This event is fired when the slice is updated
 * @extends {LitElement}
 */
@customElement('umb-donut-slice')
export class UmbDonutSliceElement extends LitElement {
	/**
	 * Number of items that this slice represents
	 *
	 * @memberof UmbDonutSliceElement
	 */
	@property({ type: Number })
	amount = 0;
	/**
	 * Color of the slice. Any valid css color is accepted, custom properties are also supported
	 *
	 * @memberof UmbDonutSliceElement
	 */
	@property()
	color = 'red';
	/**
	 * Name of the slice. This is used to display the name of the slice in the donut chart
	 *
	 * @memberof UmbDonutSliceElement
	 */
	@property()
	name = '';
	/**
	 * Kind of the slice. This is shown on a details box when hovering over the slice
	 *
	 * @memberof UmbDonutSliceElement
	 */
	@property()
	kind = '';

	willUpdate() {
		this.dispatchEvent(new CustomEvent('slice-update', { composed: true, bubbles: true }));
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-slice': UmbDonutSliceElement;
	}
}
