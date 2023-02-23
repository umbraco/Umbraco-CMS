import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, svg } from 'lit';
import { customElement, query, queryAssignedElements, state } from 'lit/decorators.js';
import { UmbDonutSliceElement } from './donut-slice';

export interface Circle {
	percent: number;
	color: string;
	name: string;
	tooltipText: string;
}

interface CircleWithCommands extends Circle {
	offset: number;
	commands: string;
}

@customElement('umb-donut-chart')
export class UmbDonutChartElement extends LitElement {
	static percentToDegrees(percent: number): number {
		return percent * 3.6;
	}

	static styles = [
		UUITextStyles,
		css`
			path {
				pointer-events: visibleFill;
			}
			.circle {
				filter: url(#erode);
			}

			.highlight {
				transition: opacity 200ms linear;
				filter: url(#filter);
				opacity: 0;
			}

			.highlight:hover {
				opacity: 0.5;
			}

			#container {
				position: relative;
				width: 200px;
			}

			#details-box {
				background: #ffffffe6;
				border: 1px solid var(--uui-color-border-standalone);
				border-radius: var(--uui-border-radius);
				box-sizing: border-box;
				top: 0;
				left: 0;
				position: absolute;
				opacity: 0;
				padding: 0.5em;
				line-height: 1.5;
				font-size: var(--uui-type-small-size);
				box-shadow: var(--uui-shadow-depth-1);
				transform: translate3d(var(--pos-x), var(--pos-y), 0);
				transition: transform 0.2s cubic-bezier(0.02, 1.23, 0.79, 1.08);
				transition: opacity 150ms linear;
			}

			#details-box.show {
				opacity: 1;
			}

			#details-box uui-icon {
				/* optically correct alignment */
				color: var(--umb-donut-detail-color);
				margin-right: 0.2em;
			}

			#details-title {
				font-weight: bold;
				display: flex;
				align-items: center;
			}
		`,
	];

	@query('slot')
	slicesSlot!: HTMLSlotElement;

	@queryAssignedElements({ selector: 'umb-donut-slice' })
	slices!: UmbDonutSliceElement[];

	@query('#circle-container')
	circleContainer!: HTMLSlotElement;

	@query('#container')
	container!: HTMLDivElement;

	@query('#details-box')
	detailsBox!: HTMLDivElement;

	@state()
	circles: CircleWithCommands[] = [];

	@state()
	radius = 45;

	@state()
	viewBox = 100;

	@state()
	borderSize = 20;

	@state()
	svgSize = 100;

	@state()
	posY = 0;

	@state()
	posX = 0;

	@state()
	detailName = '';

	@state()
	detailAmount = 0;

	@state()
	detailPercent = 0;

	@state()
	detailColor = 'red';

	#printCircles(event: Event) {
		event.stopPropagation();
		this.circles = this.#addCommands(
			this.slices.map((slice) => {
				return {
					percent: slice.percent,
					color: slice.color,
					name: slice.name,
					tooltipText: slice.tooltipText,
				};
			})
		);
	}

	#addCommands(Circles: Circle[]): CircleWithCommands[] {
		let previousPercent = 0;
		return Circles.map((slice) => {
			const sliceWithCommands: CircleWithCommands = {
				...slice,
				commands: this.#getSliceCommands(slice, this.radius, this.svgSize, this.borderSize),
				offset: previousPercent * 3.6 * -1,
			};
			previousPercent += slice.percent;
			return sliceWithCommands;
		});
	}

	#getSliceCommands(Circle: Circle, radius: number, svgSize: number, borderSize: number): string {
		const degrees = UmbDonutChartElement.percentToDegrees(Circle.percent);
		const longPathFlag = degrees > 180 ? 1 : 0;
		const innerRadius = radius - borderSize;

		const commands: string[] = [];
		commands.push(`M ${svgSize / 2 + radius} ${svgSize / 2}`);
		commands.push(`A ${radius} ${radius} 0 ${longPathFlag} 0 ${this.#getCoordFromDegrees(degrees, radius, svgSize)}`);
		commands.push(`L ${this.#getCoordFromDegrees(degrees, innerRadius, svgSize)}`);
		commands.push(`A ${innerRadius} ${innerRadius} 0 ${longPathFlag} 1 ${svgSize / 2 + innerRadius} ${svgSize / 2}`);
		return commands.join(' ');
	}

	#getCoordFromDegrees(angle: number, radius: number, svgSize: number): string {
		const x = Math.cos((angle * Math.PI) / 180);
		const y = Math.sin((angle * Math.PI) / 180);
		const coordX = x * radius + svgSize / 2;
		const coordY = y * -radius + svgSize / 2;
		return [coordX, coordY].join(' ');
	}

	#calculateDetailsBoxPosition(event: MouseEvent) {
		const rect = this.container.getBoundingClientRect();
		const x = event.clientX - rect.left;
		const y = event.clientY - rect.top;
		this.posX = x - 10;
		this.posY = y - 70;
	}

	#setDetailsBoxData(event: MouseEvent) {
		const target = event.target as SVGPathElement;
		const index = target.dataset.index as unknown as number;
		const circle = this.circles[index];
		this.detailName = circle.name;
		this.detailAmount = circle.percent;
		this.detailPercent = circle.percent;
		this.detailColor = circle.color;
	}

	#showDetailsBox(event: MouseEvent) {
		this.#setDetailsBoxData(event);
		this.detailsBox.classList.add('show');
	}

	#hideDetailsBox(event: MouseEvent) {
		this.detailsBox.classList.remove('show');
	}

	#renderCircles() {
		return svg`
				<filter id="erode" x="-20%" y="-20%" width="140%" height="140%" filterUnits="objectBoundingBox" primitiveUnits="userSpaceOnUse" color-interpolation-filters="linearRGB">
					<feMorphology operator="erode" radius="0.5 0.5" x="0%" y="0%" width="100%" height="100%" in="SourceGraphic" result="morphology"/>
				</filter>
				<filter id="filter" x="-20%" y="-20%" width="140%" height="140%" filterUnits="objectBoundingBox" primitiveUnits="userSpaceOnUse" color-interpolation-filters="linearRGB">
					<feColorMatrix 
						type="matrix" 
						values="1.8 0 0 0 0
						0 1.8 0 0 0
						0 0 1.8 0 0
						0 0 0 500 -20" x="0%" y="0%" width="100%" height="100%" in="merge1" result="colormatrix2"/>
					<feMorphology operator="erode" radius="0.5 0.5" x="0%" y="0%" width="100%" height="100%" in="colormatrix2" result="morphology2"/>
					<feFlood flood-color="#ffffff" flood-opacity="0.3" x="0%" y="0%" width="100%" height="100%" result="flood3"/>
					<feComposite in="flood3" in2="SourceAlpha" operator="in" x="0%" y="0%" width="100%" height="100%" result="composite3"/>
					<feMorphology operator="erode" radius="1 1" x="0%" y="0%" width="100%" height="100%" in="composite3" result="morphology1"/>
					<feMerge x="0%" y="0%" width="100%" height="100%" result="merge1">
						<feMergeNode in="morphology2"/>
						<feMergeNode in="morphology1"/>
					</feMerge>
					<feDropShadow stdDeviation="1 1" in="merge1" dx="0" dy="0" flood-color="#000" flood-opacity="0.8" x="0%" y="0%" width="100%" height="100%" result="dropShadow1"/>
				</filter>
				<desc>In chosen date range you have this number of log message of type: </desc>
					${this.circles.map(
						(circle, i) => svg`
								<path
								class="circle"

								data-index="${i}"
									fill="${circle.color}"
									role="listitem"
									d="${circle.commands}"
									transform="rotate(${circle.offset} ${this.viewBox / 2} ${this.viewBox / 2})">
								</path>
								<path 
								data-index="${i}"
								@mouseenter=${this.#showDetailsBox}
								@mouseleave=${this.#hideDetailsBox}
									class="highlight"
									fill="${circle.color}"
									role="listitem"
									d="${circle.commands}"
									transform="rotate(${circle.offset} ${this.viewBox / 2} ${this.viewBox / 2})">
								</path>`
					)}
		
        `;
	}

	render() {
		return html` <div id="container" @mousemove=${this.#calculateDetailsBoxPosition}>
				<svg viewBox="0 0 ${this.viewBox} ${this.viewBox}" role="list">${this.#renderCircles()}</svg>
				<div
					id="details-box"
					style="--pos-y: ${this.posY}px; --pos-x: ${this.posX}px; --umb-donut-detail-color: ${this.detailColor}">
					<div id="details-title"><uui-icon name="umb:record"></uui-icon>${this.detailName}</div>
					<span>${this.detailAmount} messages</span>
				</div>
			</div>
			<slot @slotchange=${this.#printCircles} @slice-update=${this.#printCircles}></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-chart': UmbDonutChartElement;
	}
}
