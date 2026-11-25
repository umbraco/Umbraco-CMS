import type { UmbDonutSliceElement } from './donut-slice.element.js';
import { umbFormatDonutShare } from './format-donut-share.function.js';
import { umbLayoutDonutSlices } from './layout-donut-slices.function.js';
import type { UmbDonutSliceLayout } from './layout-donut-slices.function.js';
import {
	classMap,
	css,
	customElement,
	html,
	nothing,
	property,
	query,
	queryAssignedElements,
	repeat,
	state,
	styleMap,
	svg,
	type PropertyValues,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

interface UmbDonutChartSliceData {
	element: UmbDonutSliceElement;
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

const roundCoordinate = (value: number): number => Math.round(value * 1000) / 1000;

const HOLE_MARGIN = 2;

const fitsChord = (line: DOMRect, radius: number, centreY: number): boolean => {
	const offset = Math.max(Math.abs(line.top - centreY), Math.abs(line.bottom - centreY));
	return offset < radius && line.width / 2 <= Math.sqrt(radius ** 2 - offset ** 2);
};

/**
 * A donut chart that shows each slotted `umb-donut-slice` as a share of the total of all slices.
 *
 * Each `umb-donut-slice` takes an `amount`, a `color` (any CSS colour, including `var(...)`), a `name`, a `kind` (the
 * unit shown after the amount) and optionally an `href` with a `label` for the link's accessible name. Every drawn
 * slice is a tab stop: a link when it has an `href`, otherwise a focusable image. Hovering, focusing or tapping a slice
 * shows its name, amount, kind and share in the hole of the ring, or just below the ring, over whatever follows the
 * chart, when a line does not fit in the hole; Escape hides them. Showing them never changes the size of the element.
 * Set the element's width to size the chart (it defaults to 200px); the height follows. Consumers with many small
 * categories should group the long tail themselves, for example into an "Other" slice.
 *
 * Slice colours are the consumer's to choose: give each slice at least 3:1 contrast against `--uui-color-surface` in
 * every theme. A link slice can be smaller than the 24 by 24 CSS pixels that WCAG 2.5.8 asks of a pointer target, so
 * offer an equivalent control next to the chart, for example a legend with a link per slice, or raise
 * `min-slice-percent`.
 * @element umb-donut-chart
 * @slot - The `umb-donut-slice` elements to show.
 * @cssprop --uui-color-surface - The background the chart is drawn on, the background of details shown below the
 * ring, and one of the two focus ring colours.
 * @cssprop --uui-color-text - The details text, the other focus ring colour, and the base of the empty ring colour.
 * @cssprop --uui-color-text-alt - The description text.
 * @cssprop --uui-color-border - The border of details shown below the ring.
 * @cssprop --uui-border-radius - The corners of details shown below the ring.
 * @cssprop --uui-shadow-depth-3 - The shadow of details shown below the ring.
 * @cssprop --uui-size-space-2 - Spacing around the details and above the description.
 * @cssprop --uui-size-space-3 - Spacing inside details shown below the ring.
 * @cssprop --uui-type-small-size - The details and description text size.
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

	@queryAssignedElements({ selector: 'umb-donut-slice', flatten: true })
	private _sliceElements!: UmbDonutSliceElement[];

	@query('#container', true)
	private _container!: HTMLDivElement;

	@query('#details')
	private _details?: HTMLDivElement;

	@state()
	private _activeIndex?: number;

	@state()
	private _focusIndex?: number;

	#slices: Array<UmbDonutChartSliceData> = [];
	#slicesChanged = false;
	#geometry: UmbDonutChartGeometry = {
		size: DEFAULT_SVG_SIZE,
		radius: DEFAULT_RADIUS,
		innerRadius: DEFAULT_RADIUS - DEFAULT_BORDER_SIZE,
	};
	#segments: Array<UmbDonutChartSegment> = [];
	#minSlicePercent?: number;
	#dismissed = false;
	#resizeObserver = new ResizeObserver(() => this.#placeDetails());

	#onSlicesChange = (event: Event) => {
		event.stopPropagation();
		this.#slicesChanged = true;
		this.requestUpdate();
	};

	#onPointerOver = (event: PointerEvent) => {
		const index = this.#getSegmentIndex(event.target);
		if (event.pointerType === 'touch' || index === undefined) return;
		this.#show(index);
	};

	#onPointerLeave = (event: PointerEvent) => {
		if (event.pointerType === 'touch') return;
		this._activeIndex = this.#dismissed ? undefined : this._focusIndex;
	};

	#onPointerUp = (event: PointerEvent) => {
		const index = this.#getSegmentIndex(event.target);
		if (event.pointerType !== 'touch' || !this.#isPlainSlice(index)) return;
		if (index === this._activeIndex) {
			this._activeIndex = undefined;
			return;
		}
		this.#show(index);
	};

	#onFocusIn = (event: FocusEvent) => {
		const target = event.target as Element;
		const index = this.#getSegmentIndex(target);
		if (index === undefined || !target.matches(':focus-visible')) return;
		this._focusIndex = index;
		this.#show(index);
	};

	#onFocusOut = () => {
		this._focusIndex = undefined;
		this._activeIndex = undefined;
	};

	#onKeyDown = (event: KeyboardEvent) => {
		if (event.key !== 'Escape' || !this.#detailsShown()) return;
		event.preventDefault();
		event.stopPropagation();
		this.#dismiss();
	};

	#onDocumentKeyDown = (event: KeyboardEvent) => {
		if (event.key === 'Escape' && this.#detailsShown()) this.#dismiss();
	};

	#onDocumentPointerDown = (event: PointerEvent) => {
		if (this._activeIndex !== undefined && !event.composedPath().includes(this)) this._activeIndex = undefined;
	};

	override connectedCallback(): void {
		super.connectedCallback();
		this._activeIndex = undefined;
		this._focusIndex = undefined;
		this.#dismissed = false;
		document.addEventListener('keydown', this.#onDocumentKeyDown);
		document.addEventListener('pointerdown', this.#onDocumentPointerDown);
		if (this.hasUpdated) this.#resizeObserver.observe(this._container);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		document.removeEventListener('keydown', this.#onDocumentKeyDown);
		document.removeEventListener('pointerdown', this.#onDocumentPointerDown);
		this.#resizeObserver.disconnect();
	}

	protected override firstUpdated(changedProperties: PropertyValues<this>): void {
		super.firstUpdated(changedProperties);
		this.#resizeObserver.observe(this._container);
	}

	#show(index: number) {
		this.#dismissed = false;
		this._activeIndex = index;
	}

	#dismiss() {
		this.#dismissed = true;
		this._activeIndex = undefined;
	}

	#isPlainSlice(index: number | undefined): index is number {
		return index !== undefined && !this.#segments[index]?.href;
	}

	#detailsShown(): boolean {
		return !this.hideDetailBox && this._activeIndex !== undefined;
	}

	protected override willUpdate(changedProperties: PropertyValues<this>): void {
		super.willUpdate(changedProperties);

		if (this.#updateLayoutInputs(changedProperties)) {
			this.#updateSegments();
		}
	}

	#updateLayoutInputs(changedProperties: PropertyValues<this>): boolean {
		const geometryChanged = (['radius', 'borderSize', 'svgSize'] as const).some((name) => changedProperties.has(name));
		if (geometryChanged) {
			this.#geometry = this.#resolveGeometry();
		}

		const minSlicePercentChanged = changedProperties.has('minSlicePercent');
		if (minSlicePercentChanged) {
			this.#minSlicePercent = this.#resolveMinSlicePercent();
		}

		const slicesChanged = this.#readChangedSlices();
		return geometryChanged || minSlicePercentChanged || slicesChanged;
	}

	#readChangedSlices(): boolean {
		if (!this.#slicesChanged) return false;
		this.#slicesChanged = false;
		this.#slices = this._sliceElements.map((element) => ({
			element,
			amount: element.amount,
			color: element.color,
			name: element.name,
			kind: element.kind,
			href: element.href,
			label: element.label,
		}));
		return true;
	}

	#updateSegments(): void {
		const activeSlice = this.#sliceAt(this._activeIndex);
		const focusSlice = this.#sliceAt(this._focusIndex);

		this.#segments = this.#layoutSegments();

		this._activeIndex = this.#indexOfSlice(activeSlice);
		this._focusIndex = this.#indexOfSlice(focusSlice);
	}

	#sliceAt(index: number | undefined): UmbDonutSliceElement | undefined {
		return index === undefined ? undefined : this.#segments[index]?.element;
	}

	#indexOfSlice(slice: UmbDonutSliceElement | undefined): number | undefined {
		const index = slice ? this.#segments.findIndex((segment) => segment.element === slice) : -1;
		return index === -1 ? undefined : index;
	}

	protected override updated(changedProperties: PropertyValues<this>): void {
		super.updated(changedProperties);
		this.#placeDetails();
	}

	#placeDetails() {
		const details = this._details;
		if (!details) return;
		details.classList.remove('below');
		details.classList.toggle('below', this.#geometry.innerRadius <= 0 || !this.#fitsInHole(details));
	}

	#fitsInHole(details: HTMLElement): boolean {
		const { size, innerRadius } = this.#geometry;
		const bounds = this._container.getBoundingClientRect();
		const radius = (innerRadius / size) * bounds.width - HOLE_MARGIN;
		const centreY = bounds.top + bounds.height / 2;
		const lines = details.firstElementChild?.children ?? [];
		return [...lines].every((line) => fitsChord(line.getBoundingClientRect(), radius, centreY));
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

	#resolveMinSlicePercent(): number | undefined {
		const value = this.minSlicePercent;
		if (value === undefined || value === null) return undefined;
		if (Number.isFinite(value) && value >= 0) return value;
		console.warn(
			`umb-donut-chart: min-slice-percent must be zero or a positive number, got ${value}. Using ${UmbDonutChartElement.MIN_SLICE_PERCENT}.`,
		);
		return undefined;
	}

	#layoutSegments(): Array<UmbDonutChartSegment> {
		const minSlicePercent = this.#minSlicePercent ?? UmbDonutChartElement.MIN_SLICE_PERCENT;
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

	#getAmount(segment: UmbDonutChartSegment): string {
		return [this.localize.number(segment.amount), segment.kind].filter(Boolean).join(' ');
	}

	#getShare(segment: UmbDonutChartSegment): string {
		return `(${umbFormatDonutShare(segment.share, this.localize)})`;
	}

	#getDetails(segment: UmbDonutChartSegment): string {
		return `${this.#getAmount(segment)} ${this.#getShare(segment)}`;
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
			aria-label=${segment.href ? nothing : name}
			tabindex=${segment.href ? nothing : '0'}></path>`;

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

	#renderFocusRing() {
		const segment = this._focusIndex === undefined ? undefined : this.#segments[this._focusIndex];
		if (!segment) return nothing;
		return svg`<g class="focus-ring" aria-hidden="true">
			<path class="focus-ring-outer" d=${segment.path}></path>
			<path class="focus-ring-inner" d=${segment.path}></path>
		</g>`;
	}

	#renderChart() {
		const { size } = this.#geometry;
		if (this.#segments.length === 0) {
			return html`<svg
				viewBox="0 0 ${size} ${size}"
				role=${this.description ? 'img' : nothing}
				aria-label=${this.description || nothing}
				aria-hidden=${this.description ? nothing : 'true'}>
				<path class="track" fill-rule="evenodd" d=${this.#getRingPath()}></path>
			</svg>`;
		}

		return html`<svg viewBox="0 0 ${size} ${size}" role="list" aria-label=${this.description || nothing}>
			${repeat(
				this.#segments,
				(segment) => segment.element,
				(segment, index) => this.#renderSegment(segment, index),
			)}${this.#renderFocusRing()}
		</svg>`;
	}

	#renderDetails() {
		const segment = this._activeIndex === undefined ? undefined : this.#segments[this._activeIndex];
		if (this.hideDetailBox || !segment) return nothing;
		return html`<div id="details" aria-hidden="true">
			<div id="details-panel">
				<div id="details-title">
					<span class="swatch" style=${styleMap({ background: segment.color })}></span>${segment.name}
				</div>
				<div>${this.#getAmount(segment)}</div>
				<div>${this.#getShare(segment)}</div>
			</div>
		</div>`;
	}

	override render() {
		return html`<div
				id="chart"
				@pointerover=${this.#onPointerOver}
				@pointerleave=${this.#onPointerLeave}
				@pointerup=${this.#onPointerUp}
				@focusin=${this.#onFocusIn}
				@focusout=${this.#onFocusOut}
				@keydown=${this.#onKeyDown}>
				<div id="container">${this.#renderChart()}</div>
				${this.#renderDetails()}
			</div>
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

			#chart {
				position: relative;
			}

			#container {
				aspect-ratio: 1;
			}

			svg {
				display: block;
				width: 100%;
				height: 100%;
				overflow: visible;
			}

			.track {
				fill: color-mix(in srgb, var(--uui-color-text) 45%, var(--uui-color-surface));
			}

			.segment {
				forced-color-adjust: none;
				transform-box: view-box;
				transform-origin: center;
				transition: transform 120ms ease-out;
			}

			.segment.active {
				transform: scale(1.04);
			}

			a,
			.slice {
				outline: none;
			}

			.focus-ring path {
				fill: none;
				pointer-events: none;
				stroke-linejoin: round;
				vector-effect: non-scaling-stroke;
			}

			.focus-ring-outer {
				stroke: var(--uui-color-surface);
				stroke-width: 6px;
			}

			.focus-ring-inner {
				stroke: var(--uui-color-text);
				stroke-width: 2px;
			}

			#details {
				position: absolute;
				top: 50%;
				left: 50%;
				translate: -50% -50%;
				box-sizing: border-box;
				color: var(--uui-color-text);
				font-size: var(--uui-type-small-size);
				line-height: 1.5;
				text-align: center;
				white-space: nowrap;
			}

			#details-panel {
				display: flex;
				flex-direction: column;
				align-items: center;
				box-sizing: border-box;
			}

			#details.below {
				top: 100%;
				right: 0;
				left: 0;
				z-index: 1;
				translate: none;
				padding-top: var(--uui-size-space-2);
				white-space: normal;
				overflow-wrap: anywhere;
			}

			#details.below #details-panel {
				padding: var(--uui-size-space-2) var(--uui-size-space-3);
				background: var(--uui-color-surface);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
			}

			#details-title {
				display: flex;
				align-items: center;
				justify-content: center;
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
				.segment {
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
