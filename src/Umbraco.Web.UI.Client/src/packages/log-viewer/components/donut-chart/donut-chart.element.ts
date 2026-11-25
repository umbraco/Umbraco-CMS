import type { UmbDonutSliceElement } from './donut-slice.element.js';
import { umbFormatDonutShare } from './format-donut-share.function.js';
import { umbLayoutDonutSlices } from './layout-donut-slices.function.js';
import type { UmbDonutSliceLayout } from './layout-donut-slices.function.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	classMap,
	css,
	customElement,
	html,
	nothing,
	property,
	query,
	queryAssignedElements,
	state,
	styleMap,
	svg,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

interface UmbDonutChartSliceData {
	amount: number;
	color: string;
	name: string;
	kind: string;
	href: string;
	label: string;
}

interface UmbDonutChartSegment extends UmbDonutChartSliceData, UmbDonutSliceLayout {
	path: string;
}

interface UmbDonutChartGeometry {
	size: number;
	radius: number;
	innerRadius: number;
}

const DEFAULT_SVG_SIZE = 100;
const DEFAULT_RADIUS = 45;
const DEFAULT_BORDER_SIZE = 20;
const SLICE_GAP_DEGREES = 1.5;
const DETAILS_BOX_OFFSET = 12;

const roundCoordinate = (value: number): number => Math.round(value * 1000) / 1000;

const placeWithin = (anchor: number, length: number, available: number): number => {
	const after = anchor + DETAILS_BOX_OFFSET;
	const preferred = after + length <= available ? after : anchor - DETAILS_BOX_OFFSET - length;
	return Math.min(Math.max(preferred, 0), Math.max(available - length, 0));
};

/**
 * A donut chart that shows each slotted `umb-donut-slice` as a share of the total of all slices.
 * Set the element's width to size the chart; it defaults to 200px.
 * @element umb-donut-chart
 * @slot - The `umb-donut-slice` elements to show.
 */
@customElement('umb-donut-chart')
export class UmbDonutChartElement extends UmbLitElement {
	/**
	 * Converts a percentage of the full circle to degrees.
	 * @param {number} percent - The percentage of the full circle.
	 * @returns {number} The angle in degrees.
	 */
	static percentToDegrees(percent: number): number {
		return percent * 3.6;
	}

	/**
	 * The default for `minSlicePercent`: the smallest share of the ring, in percent, that a slice with an amount is
	 * drawn with, so it stays visible and can be hovered.
	 */
	static MIN_SLICE_PERCENT = 1;

	/**
	 * Circle radius in SVG user units (see `svgSize`)
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number })
	radius = DEFAULT_RADIUS;

	/**
	 * The circle thickness in SVG user units. Clamped to `radius`.
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'border-size' })
	borderSize = DEFAULT_BORDER_SIZE;

	/**
	 * The width and height of the SVG viewBox in user units. The rendered size follows the element's width.
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'svg-size' })
	svgSize = DEFAULT_SVG_SIZE;

	/**
	 * Description of the graph, used as the accessible name of the list of slices
	 * @memberof UmbDonutChartElement
	 */
	@property()
	description = '';

	/**
	 * Hides the box that appears on hover or keyboard focus with the details of the slice
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Boolean })
	hideDetailBox = false;

	/**
	 * Shows the description text below the chart
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Boolean, attribute: 'show-description' })
	showDescription = false;

	/**
	 * The smallest share of the ring, in percent, that a slice with an amount is drawn with. It shrinks when there are
	 * too many slices for each to get it. Defaults to `UmbDonutChartElement.MIN_SLICE_PERCENT`. The details box and
	 * accessible names always use the true share.
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'min-slice-percent' })
	minSlicePercent?: number;

	@queryAssignedElements({ selector: 'umb-donut-slice' })
	private _sliceElements!: UmbDonutSliceElement[];

	@query('#container')
	private _container!: HTMLDivElement;

	@query('#details-box')
	private _detailsBox?: HTMLDivElement;

	@state()
	private _activeIndex?: number;

	#slices: Array<UmbDonutChartSliceData> = [];
	#slicesChanged = false;
	#geometry: UmbDonutChartGeometry = {
		size: DEFAULT_SVG_SIZE,
		radius: DEFAULT_RADIUS,
		innerRadius: DEFAULT_RADIUS - DEFAULT_BORDER_SIZE,
	};
	#segments: Array<UmbDonutChartSegment> = [];
	#detailsIndex?: number;
	#detailsBoxAnchor = { x: 0, y: 0 };

	#onSlicesChange = (event: Event) => {
		event.stopPropagation();
		this.#slicesChanged = true;
		this.requestUpdate();
	};

	#onPointerOver = (event: PointerEvent) => {
		this._activeIndex = this.#getSegmentIndex(event.target);
		this.#setDetailsBoxAnchorFromPointer(event);
	};

	#onPointerMove = (event: PointerEvent) => {
		if (this._activeIndex === undefined) return;
		this.#setDetailsBoxAnchorFromPointer(event);
		this.#positionDetailsBox();
	};

	#onPointerLeave = () => {
		const focused = this.shadowRoot?.activeElement;
		this._activeIndex = focused?.matches(':focus-visible') ? this.#getSegmentIndex(focused) : undefined;
		if (this._activeIndex !== undefined) this.#setDetailsBoxAnchorFromSegment(this._activeIndex);
	};

	#onFocusIn = (event: FocusEvent) => {
		const target = event.target as Element;
		if (!target.matches(':focus-visible')) return;
		this._activeIndex = this.#getSegmentIndex(target);
		if (this._activeIndex !== undefined) this.#setDetailsBoxAnchorFromSegment(this._activeIndex);
	};

	#onFocusOut = () => {
		this._activeIndex = undefined;
	};

	protected override willUpdate(changedProperties: Map<PropertyKey, unknown>): void {
		super.willUpdate(changedProperties);

		const geometryChanged =
			changedProperties.has('radius') || changedProperties.has('borderSize') || changedProperties.has('svgSize');
		if (geometryChanged) {
			this.#geometry = this.#resolveGeometry();
		}

		const slicesChanged = this.#slicesChanged;
		if (slicesChanged) {
			this.#slicesChanged = false;
			this.#slices = this._sliceElements.map(({ amount, color, name, kind, href, label }) => ({
				amount,
				color,
				name,
				kind,
				href,
				label,
			}));
		}

		if (geometryChanged || slicesChanged || changedProperties.has('minSlicePercent')) {
			this.#segments = this.#layoutSegments();
			if (this._activeIndex !== undefined && this._activeIndex >= this.#segments.length) {
				this._activeIndex = undefined;
			}
		}

		if (this._activeIndex !== undefined) {
			this.#detailsIndex = this._activeIndex;
		}
	}

	protected override updated(changedProperties: Map<PropertyKey, unknown>): void {
		super.updated(changedProperties);
		if (this._activeIndex !== undefined) {
			this.#positionDetailsBox();
		}
	}

	#resolveGeometry(): UmbDonutChartGeometry {
		const size = this.#positiveOr(this.svgSize, DEFAULT_SVG_SIZE, 'svg-size');

		let radius = this.#positiveOr(this.radius, DEFAULT_RADIUS, 'radius');
		if (radius > size / 2) {
			console.warn(`umb-donut-chart: radius (${radius}) does not fit svg-size (${size}). Using ${size / 2}.`);
			radius = size / 2;
		}

		let borderSize = this.#positiveOr(this.borderSize, DEFAULT_BORDER_SIZE, 'border-size');
		if (borderSize > radius) {
			console.warn(`umb-donut-chart: border-size (${borderSize}) is larger than radius (${radius}). Using ${radius}.`);
			borderSize = radius;
		}

		return { size, radius, innerRadius: radius - borderSize };
	}

	#positiveOr(value: number, fallback: number, name: string): number {
		if (Number.isFinite(value) && value > 0) return value;
		console.warn(`umb-donut-chart: ${name} must be a positive number, got ${value}. Using ${fallback}.`);
		return fallback;
	}

	#layoutSegments(): Array<UmbDonutChartSegment> {
		const minSlicePercent = this.minSlicePercent ?? UmbDonutChartElement.MIN_SLICE_PERCENT;
		const layout = umbLayoutDonutSlices(
			this.#slices.map((slice) => slice.amount),
			{ minSweep: UmbDonutChartElement.percentToDegrees(minSlicePercent), gap: SLICE_GAP_DEGREES },
		);
		return layout.map((slice) => ({ ...this.#slices[slice.index], ...slice, path: this.#getSlicePath(slice) }));
	}

	#getSlicePath({ startAngle, sweep }: UmbDonutSliceLayout): string {
		if (sweep >= 360) return this.#getRingPath();

		const { radius, innerRadius } = this.#geometry;
		const endAngle = startAngle + sweep;
		const largeArc = sweep > 180 ? 1 : 0;
		return [
			`M ${this.#getPoint(startAngle, radius)}`,
			`A ${radius} ${radius} 0 ${largeArc} 1 ${this.#getPoint(endAngle, radius)}`,
			`L ${this.#getPoint(endAngle, innerRadius)}`,
			`A ${innerRadius} ${innerRadius} 0 ${largeArc} 0 ${this.#getPoint(startAngle, innerRadius)}`,
			'Z',
		].join(' ');
	}

	#getRingPath(): string {
		const { radius, innerRadius } = this.#geometry;
		const circle = (r: number, sweepFlag: number) =>
			`M ${this.#getPoint(0, r)} A ${r} ${r} 0 1 ${sweepFlag} ${this.#getPoint(180, r)} A ${r} ${r} 0 1 ${sweepFlag} ${this.#getPoint(0, r)} Z`;
		return `${circle(radius, 1)} ${circle(innerRadius, 0)}`;
	}

	#getPoint(angle: number, distance: number): string {
		const { x, y } = this.#getCoordinates(angle, distance);
		return `${roundCoordinate(x)} ${roundCoordinate(y)}`;
	}

	#getCoordinates(angle: number, distance: number) {
		const center = this.#geometry.size / 2;
		const radians = (angle * Math.PI) / 180;
		return { x: center + distance * Math.sin(radians), y: center - distance * Math.cos(radians) };
	}

	#getSegmentIndex(target: EventTarget | null): number | undefined {
		const index = target instanceof Element ? target.closest('[data-index]')?.getAttribute('data-index') : null;
		return index ? Number(index) : undefined;
	}

	#setDetailsBoxAnchorFromPointer(event: PointerEvent) {
		const bounds = this._container.getBoundingClientRect();
		this.#detailsBoxAnchor = { x: event.clientX - bounds.left, y: event.clientY - bounds.top };
	}

	#setDetailsBoxAnchorFromSegment(index: number) {
		const segment = this.#segments[index];
		if (!segment) return;
		const { size, radius, innerRadius } = this.#geometry;
		const scale = this._container.clientWidth / size;
		const { x, y } = this.#getCoordinates(segment.startAngle + segment.sweep / 2, (radius + innerRadius) / 2);
		this.#detailsBoxAnchor = { x: x * scale, y: y * scale };
	}

	#positionDetailsBox() {
		if (!this._detailsBox) return;
		const { x, y } = this.#detailsBoxAnchor;
		const left = placeWithin(x, this._detailsBox.offsetWidth, this._container.clientWidth);
		const top = placeWithin(y, this._detailsBox.offsetHeight, this._container.clientHeight);
		this._detailsBox.style.translate = `${left}px ${top}px`;
	}

	#getDetails(segment: UmbDonutChartSegment): string {
		const amount = [this.localize.number(segment.amount), segment.kind].filter(Boolean).join(' ');
		return `${amount} (${umbFormatDonutShare(segment.share, this.localize)})`;
	}

	#getAccessibleName(segment: UmbDonutChartSegment): string {
		const details = this.#getDetails(segment);
		return segment.name ? `${segment.name}: ${details}` : details;
	}

	#renderSegment(segment: UmbDonutChartSegment, index: number) {
		const name = this.#getAccessibleName(segment);
		const slice = svg`<path
			class="slice"
			fill-rule="evenodd"
			d=${segment.path}
			style=${styleMap({ fill: segment.color })}
			role=${segment.href ? nothing : 'img'}
			aria-label=${segment.href ? nothing : name}></path>`;

		return svg`<g
			role="listitem"
			class=${classMap({ segment: true, active: index === this._activeIndex })}
			data-index=${index}>
			${
				segment.href
					? svg`<a href=${segment.href} aria-label=${segment.label || name}>
						${segment.label ? svg`<desc>${name}</desc>` : nothing}${slice}
					</a>`
					: slice
			}
		</g>`;
	}

	#renderChart() {
		const { size } = this.#geometry;
		if (this.#segments.length === 0) {
			return html`<svg viewBox="0 0 ${size} ${size}" aria-hidden="true">
				<path class="track" fill-rule="evenodd" d=${this.#getRingPath()}></path>
			</svg>`;
		}

		return html`<svg
			viewBox="0 0 ${size} ${size}"
			role="list"
			aria-label=${this.description || nothing}
			class=${classMap({ 'has-active': this._activeIndex !== undefined })}
			@pointerover=${this.#onPointerOver}
			@pointermove=${this.#onPointerMove}
			@pointerleave=${this.#onPointerLeave}
			@focusin=${this.#onFocusIn}
			@focusout=${this.#onFocusOut}>
			${this.#segments.map((segment, index) => this.#renderSegment(segment, index))}
		</svg>`;
	}

	#renderDetailsBox() {
		if (this.hideDetailBox) return nothing;
		const segment = this.#detailsIndex === undefined ? undefined : this.#segments[this.#detailsIndex];
		return html`<div id="details-box" class=${classMap({ show: this._activeIndex !== undefined })} aria-hidden="true">
			${segment
				? html`<div id="details-title">
							<span class="swatch" style=${styleMap({ background: segment.color })}></span>${segment.name}
						</div>
						<div>${this.#getDetails(segment)}</div>`
				: nothing}
		</div>`;
	}

	override render() {
		return html`<div id="container">${this.#renderChart()}${this.#renderDetailsBox()}</div>
			${this.showDescription && this.description ? html`<p class="description">${this.description}</p>` : nothing}
			<slot @slotchange=${this.#onSlicesChange} @slice-update=${this.#onSlicesChange}></slot>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 200px;
				max-width: 100%;
			}

			#container {
				position: relative;
				aspect-ratio: 1;
			}

			svg {
				display: block;
				width: 100%;
				height: 100%;
				overflow: visible;
			}

			.track {
				fill: var(--uui-color-border);
			}

			.segment {
				transition: opacity 120ms linear;
			}

			svg.has-active .segment:not(.active) {
				opacity: 0.7;
			}

			a {
				outline: none;
			}

			a:focus-visible .slice {
				stroke: var(--uui-color-focus);
				stroke-width: 2px;
				vector-effect: non-scaling-stroke;
			}

			#details-box {
				position: absolute;
				top: 0;
				left: 0;
				z-index: 1;
				box-sizing: border-box;
				width: max-content;
				max-width: 100%;
				padding: var(--uui-size-space-2) var(--uui-size-space-3);
				background: var(--uui-color-surface);
				color: var(--uui-color-text);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
				font-size: var(--uui-type-small-size);
				line-height: 1.5;
				opacity: 0;
				pointer-events: none;
				transition: opacity 120ms linear;
			}

			#details-box.show {
				opacity: 1;
			}

			#details-title {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
				font-weight: bold;
			}

			.swatch {
				flex: none;
				width: 0.75em;
				height: 0.75em;
				border-radius: 50%;
				forced-color-adjust: none;
			}

			.description {
				text-align: center;
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
				margin: var(--uui-size-space-2) 0 0 0;
			}

			@media (prefers-reduced-motion: reduce) {
				.segment,
				#details-box {
					transition: none;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-chart': UmbDonutChartElement;
	}
}
