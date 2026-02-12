import type { UmbTiptapCharacterMapModalData, UmbTiptapCharacterMapModalValue } from './character-map-modal.token.js';
import { css, customElement, html, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-character-map-modal')
export class UmbCharacterMapModalElement extends UmbModalBaseElement<
	UmbTiptapCharacterMapModalData,
	UmbTiptapCharacterMapModalValue
> {
	/* The character mapping code has been derived from TinyMCE.
	 * https://github.com/tinymce/tinymce/blob/6.8.5/modules/tinymce/src/plugins/charmap/main/ts/core/CharMap.ts#L20-L362
	 * SPDX-License-Identifier: MIT
	 * Copyright © 2022 Ephox Corporation DBA Tiny Technologies, Inc.
	 * Modifications are licensed under the MIT License. */
	#characterMap: Record<string, Array<[number, string]>> = {
		'#general_all': [],
		'#tiptap_charmap_currency': [
			[36, 'dollar sign'],
			[162, 'cent sign'],
			[8364, 'euro sign'],
			[163, 'pound sign'],
			[165, 'yen sign'],
			[164, 'currency sign'],
			[8352, 'euro-currency sign'],
			[8353, 'colon sign'],
			[8354, 'cruzeiro sign'],
			[8355, 'french franc sign'],
			[8356, 'lira sign'],
			[8357, 'mill sign'],
			[8358, 'naira sign'],
			[8359, 'peseta sign'],
			[8360, 'rupee sign'],
			[8361, 'won sign'],
			[8362, 'new sheqel sign'],
			[8363, 'dong sign'],
			[8365, 'kip sign'],
			[8366, 'tugrik sign'],
			[8367, 'drachma sign'],
			[8368, 'german penny symbol'],
			[8369, 'peso sign'],
			[8370, 'guarani sign'],
			[8371, 'austral sign'],
			[8372, 'hryvnia sign'],
			[8373, 'cedi sign'],
			[8374, 'livre tournois sign'],
			[8375, 'spesmilo sign'],
			[8376, 'tenge sign'],
			[8377, 'indian rupee sign'],
			[8378, 'turkish lira sign'],
			[8379, 'nordic mark sign'],
			[8380, 'manat sign'],
			[8381, 'ruble sign'],
			[20870, 'yen character'],
			[20803, 'yuan character'],
			[22291, 'yuan character, in hong kong and taiwan'],
			[22278, 'yen/yuan character variant one'],
		],
		'#tiptap_charmap_text': [
			[169, 'copyright sign'],
			[174, 'registered sign'],
			[8482, 'trade mark sign'],
			[8240, 'per mille sign'],
			[181, 'micro sign'],
			[183, 'middle dot'],
			[8226, 'bullet'],
			[8230, 'three dot leader'],
			[8242, 'minutes / feet'],
			[8243, 'seconds / inches'],
			[167, 'section sign'],
			[182, 'paragraph sign'],
			[223, 'sharp s / ess-zed'],
		],
		'#tiptap_charmap_quotations': [
			[8249, 'single left-pointing angle quotation mark'],
			[8250, 'single right-pointing angle quotation mark'],
			[171, 'left pointing guillemet'],
			[187, 'right pointing guillemet'],
			[8216, 'left single quotation mark'],
			[8217, 'right single quotation mark'],
			[8220, 'left double quotation mark'],
			[8221, 'right double quotation mark'],
			[8218, 'single low-9 quotation mark'],
			[8222, 'double low-9 quotation mark'],
			[60, 'less-than sign'],
			[62, 'greater-than sign'],
			[8804, 'less-than or equal to'],
			[8805, 'greater-than or equal to'],
			[8211, 'en dash'],
			[8212, 'em dash'],
			[175, 'macron'],
			[8254, 'overline'],
			[164, 'currency sign'],
			[166, 'broken bar'],
			[168, 'diaeresis'],
			[161, 'inverted exclamation mark'],
			[191, 'turned question mark'],
			[710, 'circumflex accent'],
			[732, 'small tilde'],
			[176, 'degree sign'],
			[8722, 'minus sign'],
			[177, 'plus-minus sign'],
			[247, 'division sign'],
			[8260, 'fraction slash'],
			[215, 'multiplication sign'],
			[185, 'superscript one'],
			[178, 'superscript two'],
			[179, 'superscript three'],
			[188, 'fraction one quarter'],
			[189, 'fraction one half'],
			[190, 'fraction three quarters'],
		],
		'#tiptap_charmap_maths': [
			[402, 'function / florin'],
			[8747, 'integral'],
			[8721, 'n-ary sumation'],
			[8734, 'infinity'],
			[8730, 'square root'],
			[8764, 'similar to'],
			[8773, 'approximately equal to'],
			[8776, 'almost equal to'],
			[8800, 'not equal to'],
			[8801, 'identical to'],
			[8712, 'element of'],
			[8713, 'not an element of'],
			[8715, 'contains as member'],
			[8719, 'n-ary product'],
			[8743, 'logical and'],
			[8744, 'logical or'],
			[172, 'not sign'],
			[8745, 'intersection'],
			[8746, 'union'],
			[8706, 'partial differential'],
			[8704, 'for all'],
			[8707, 'there exists'],
			[8709, 'diameter'],
			[8711, 'backward difference'],
			[8727, 'asterisk operator'],
			[8733, 'proportional to'],
			[8736, 'angle'],
		],
		'#tiptap_charmap_extlatin': [
			[192, 'A - grave'],
			[193, 'A - acute'],
			[194, 'A - circumflex'],
			[195, 'A - tilde'],
			[196, 'A - diaeresis'],
			[197, 'A - ring above'],
			[256, 'A - macron'],
			[198, 'ligature AE'],
			[199, 'C - cedilla'],
			[200, 'E - grave'],
			[201, 'E - acute'],
			[202, 'E - circumflex'],
			[203, 'E - diaeresis'],
			[274, 'E - macron'],
			[204, 'I - grave'],
			[205, 'I - acute'],
			[206, 'I - circumflex'],
			[207, 'I - diaeresis'],
			[298, 'I - macron'],
			[208, 'ETH'],
			[209, 'N - tilde'],
			[210, 'O - grave'],
			[211, 'O - acute'],
			[212, 'O - circumflex'],
			[213, 'O - tilde'],
			[214, 'O - diaeresis'],
			[216, 'O - slash'],
			[332, 'O - macron'],
			[338, 'ligature OE'],
			[352, 'S - caron'],
			[217, 'U - grave'],
			[218, 'U - acute'],
			[219, 'U - circumflex'],
			[220, 'U - diaeresis'],
			[362, 'U - macron'],
			[221, 'Y - acute'],
			[376, 'Y - diaeresis'],
			[562, 'Y - macron'],
			[222, 'THORN'],
			[224, 'a - grave'],
			[225, 'a - acute'],
			[226, 'a - circumflex'],
			[227, 'a - tilde'],
			[228, 'a - diaeresis'],
			[229, 'a - ring above'],
			[257, 'a - macron'],
			[230, 'ligature ae'],
			[231, 'c - cedilla'],
			[232, 'e - grave'],
			[233, 'e - acute'],
			[234, 'e - circumflex'],
			[235, 'e - diaeresis'],
			[275, 'e - macron'],
			[236, 'i - grave'],
			[237, 'i - acute'],
			[238, 'i - circumflex'],
			[239, 'i - diaeresis'],
			[299, 'i - macron'],
			[240, 'eth'],
			[241, 'n - tilde'],
			[242, 'o - grave'],
			[243, 'o - acute'],
			[244, 'o - circumflex'],
			[245, 'o - tilde'],
			[246, 'o - diaeresis'],
			[248, 'o slash'],
			[333, 'o macron'],
			[339, 'ligature oe'],
			[353, 's - caron'],
			[249, 'u - grave'],
			[250, 'u - acute'],
			[251, 'u - circumflex'],
			[252, 'u - diaeresis'],
			[363, 'u - macron'],
			[253, 'y - acute'],
			[254, 'thorn'],
			[255, 'y - diaeresis'],
			[563, 'y - macron'],
			[913, 'Alpha'],
			[914, 'Beta'],
			[915, 'Gamma'],
			[916, 'Delta'],
			[917, 'Epsilon'],
			[918, 'Zeta'],
			[919, 'Eta'],
			[920, 'Theta'],
			[921, 'Iota'],
			[922, 'Kappa'],
			[923, 'Lambda'],
			[924, 'Mu'],
			[925, 'Nu'],
			[926, 'Xi'],
			[927, 'Omicron'],
			[928, 'Pi'],
			[929, 'Rho'],
			[931, 'Sigma'],
			[932, 'Tau'],
			[933, 'Upsilon'],
			[934, 'Phi'],
			[935, 'Chi'],
			[936, 'Psi'],
			[937, 'Omega'],
			[945, 'alpha'],
			[946, 'beta'],
			[947, 'gamma'],
			[948, 'delta'],
			[949, 'epsilon'],
			[950, 'zeta'],
			[951, 'eta'],
			[952, 'theta'],
			[953, 'iota'],
			[954, 'kappa'],
			[955, 'lambda'],
			[956, 'mu'],
			[957, 'nu'],
			[958, 'xi'],
			[959, 'omicron'],
			[960, 'pi'],
			[961, 'rho'],
			[962, 'final sigma'],
			[963, 'sigma'],
			[964, 'tau'],
			[965, 'upsilon'],
			[966, 'phi'],
			[967, 'chi'],
			[968, 'psi'],
			[969, 'omega'],
		],
		'#tiptap_charmap_symbols': [
			[8501, 'alef symbol'],
			[982, 'pi symbol'],
			[8476, 'real part symbol'],
			[978, 'upsilon - hook symbol'],
			[8472, 'Weierstrass p'],
			[8465, 'imaginary part'],
		],
		'#tiptap_charmap_arrows': [
			[8592, 'leftwards arrow'],
			[8593, 'upwards arrow'],
			[8594, 'rightwards arrow'],
			[8595, 'downwards arrow'],
			[8596, 'left right arrow'],
			[8629, 'carriage return'],
			[8656, 'leftwards double arrow'],
			[8657, 'upwards double arrow'],
			[8658, 'rightwards double arrow'],
			[8659, 'downwards double arrow'],
			[8660, 'left right double arrow'],
			[8756, 'therefore'],
			[8834, 'subset of'],
			[8835, 'superset of'],
			[8836, 'not a subset of'],
			[8838, 'subset of or equal to'],
			[8839, 'superset of or equal to'],
			[8853, 'circled plus'],
			[8855, 'circled times'],
			[8869, 'perpendicular'],
			[8901, 'dot operator'],
			[8968, 'left ceiling'],
			[8969, 'right ceiling'],
			[8970, 'left floor'],
			[8971, 'right floor'],
			[9001, 'left-pointing angle bracket'],
			[9002, 'right-pointing angle bracket'],
			[9674, 'lozenge'],
			[9824, 'black spade suit'],
			[9827, 'black club suit'],
			[9829, 'black heart suit'],
			[9830, 'black diamond suit'],
			[8194, 'en space'],
			[8195, 'em space'],
			[8201, 'thin space'],
			[8204, 'zero width non-joiner'],
			[8205, 'zero width joiner'],
			[8206, 'left-to-right mark'],
			[8207, 'right-to-left mark'],
		],
	};

	#placeholderLabel = this.localize.term('placeholders_filter');

	@state()
	private _filterQuery = '';

	@state()
	private _hoverLabel: string = '';

	@state()
	private _selectedGroup: string = '#general_all';

	#onClickCharacter(code: number) {
		this.value = String.fromCharCode(code);
		this._submitModal();
	}

	#onFilterInput(event: InputEvent & { target: HTMLInputElement }) {
		this._filterQuery = (event.target.value ?? '').toLocaleLowerCase();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('tiptap_charmap_headline')}>
				${this.#renderCharacterMap()}
				<uui-button
					slot="actions"
					label=${this.localize.term('general_close')}
					@click=${this._rejectModal}></uui-button>
			</umb-body-layout>
		`;
	}

	#filterHandler = ([, label]: [number, string]) =>
		!this._filterQuery || label.toLocaleLowerCase().includes(this._filterQuery);

	#renderCharacterMap() {
		const characters =
			this._selectedGroup && this._selectedGroup !== '#general_all'
				? this.#characterMap[this._selectedGroup].filter(this.#filterHandler)
				: Object.values(this.#characterMap).flat().filter(this.#filterHandler);

		return html`
			<div id="container">
				<div>
					${repeat(
						Object.keys(this.#characterMap),
						(group) => group,
						(group) => html`
							<uui-menu-item
								label=${this.localize.string(group)}
								?active=${this._selectedGroup === group}
								@click-label=${() => (this._selectedGroup = group)}></uui-menu-item>
						`,
					)}
				</div>
				<div id="main">
					<uui-input
						type="search"
						autocomplete="off"
						label=${this.#placeholderLabel}
						placeholder=${this.#placeholderLabel}
						@input=${this.#onFilterInput}
						${umbFocus()}>
						<div slot="prepend">
							<uui-icon name="search"></uui-icon>
						</div>
					</uui-input>
					<uui-scroll-container>
						${when(
							characters?.length,
							() => html`
								<div id="characters">
									${repeat(
										characters,
										([code]) => code,
										([code, label]) => html`
											<uui-button
												label=${label}
												title=${label}
												@click=${() => this.#onClickCharacter(code)}
												@mouseover=${() => (this._hoverLabel = label)}
												@mouseleave=${() => (this._hoverLabel = '')}>
												<span>${String.fromCharCode(code)}</span>
											</uui-button>
										`,
									)}
								</div>
							`,
							() => html`<p><umb-localize key="content_noItemsToShow">There are no items to show</umb-localize></p>`,
						)}
					</uui-scroll-container>
				</div>
			</div>
			<div slot="footer-info">${this._hoverLabel}</div>
		`;
	}

	static override styles = [
		css`
			:host {
				--umb-body-layout-color-background: var(--uui-color-surface);
				--uui-menu-item-flat-structure: 1;
			}

			#container {
				display: grid;
				grid-template-columns: var(--uui-size-48) 1fr;
				gap: var(--uui-size-layout-1);
			}

			#main {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}

			uui-scroll-container {
				height: 300px;
				width: calc(450px + var(--uui-size-layout-1));
			}

			#characters {
				display: grid;
				grid-template-columns: repeat(auto-fill, var(--uui-size-14));
				gap: var(--uui-size-5);
				padding: var(--uui-size-1);

				uui-button {
					--uui-button-font-weight: normal;

					border-radius: var(--uui-border-radius);
					font-size: 1.5rem;

					&:focus,
					&:hover {
						outline: 2px solid var(--uui-color-selected);
					}
				}
			}

			div[slot='footer-info'] {
				margin-left: var(--uui-size-layout-1);
				text-transform: capitalize;
			}
		`,
	];
}

export { UmbCharacterMapModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-character-map-modal': UmbCharacterMapModalElement;
	}
}
