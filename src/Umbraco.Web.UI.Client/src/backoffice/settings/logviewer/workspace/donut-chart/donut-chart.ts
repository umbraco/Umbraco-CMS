import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, svg } from 'lit';
import { customElement, query, queryAssignedElements, state } from 'lit/decorators.js';
import { UmbDonutSliceElement } from './donut-slice';

export interface Circle {
	percent: number;
	color: string;
}

interface CircleWithCommands extends Circle {
	offset: number;
	commands: string;
}

@customElement('umb-donut-chart')
export class UmbDonutChartElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			.slice {
				stroke-linecap: round;
				stroke-width: 2;
				fill: none;
				cursor: pointer;
			}

			.do-shit-on-hover:hover {
				fill: pink;
			}
		`,
	];

	@query('slot')
	slicesSlot!: HTMLSlotElement;

	@queryAssignedElements({ selector: 'umb-donut-slice' })
	slices!: UmbDonutSliceElement[];

	@query('#circle-container')
	circleContainer!: HTMLSlotElement;

	@state()
	circles: CircleWithCommands[] = [];

	@state()
	radius = 50;

	@state()
	viewBox = 100;

	@state()
	borderSize = 20;

	@state() svgSize = 100;

	#printCircles() {
		this.circles = this.#addCommands(
			this.slices.map((slice) => {
				return {
					percent: slice.percent,
					color: slice.color,
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

	static percentToDegrees(percent: number): number {
		return percent * 3.6;
	}

	#renderCircles() {
		return svg`
        	<svg viewBox="0 0 ${this.viewBox} ${this.viewBox}">
				${this.circles.map(
					(circle) => svg`
						<path 
							class="do-shit-on-hover" 
							fill="${circle.color}" 
							d="${circle.commands}" 
							transform="rotate(${circle.offset} ${this.viewBox / 2} ${this.viewBox / 2})">
							<title>${circle.color}</title>
						</path>`
				)}
		</svg>
        `;
	}

	render() {
		return html` <div style="width: 200px">${this.#renderCircles()}</div>
			<slot @slotchange=${this.#printCircles}></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-chart': UmbDonutChartElement;
	}
}
