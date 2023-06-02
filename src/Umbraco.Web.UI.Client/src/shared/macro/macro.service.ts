import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export interface MacroSyntaxData {
	macroAlias: string;
	macroParamsDictionary: { [key: string]: string };
	syntax?: string;
}

export class UmbMacroService {
	/** parses the special macro syntax like <?UMBRACO_MACRO macroAlias="Map" />
	 * and returns an object with the macro alias and it's parameters
	 * */
	parseMacroSyntax(syntax = '') {
		//This regex will match an alias of anything except characters that are quotes or new lines (for legacy reasons, when new macros are created
		// their aliases are cleaned an invalid chars are stripped)
		const expression =
			/(<\?UMBRACO_MACRO (?:.+?)?macroAlias=["']([^"'\n\r]+?)["'][\s\S]+?)(\/>|>.*?<\/\?UMBRACO_MACRO>)/i;
		const match = expression.exec(syntax);

		if (!match || match.length < 3) {
			return null;
		}

		const macroAlias = match[2];

		//this will leave us with just the parameters
		const paramsChunk = match[1]
			.trim()
			.replace(new RegExp(`UMBRACO_MACRO macroAlias=["']${macroAlias}["']`), '')
			.trim();
		const paramExpression = /(\w+?)=['"]([\s\S]*?)['"]/g;

		const returnVal: MacroSyntaxData = {
			macroAlias,
			macroParamsDictionary: {},
		};

		let paramMatch;
		while ((paramMatch = paramExpression.exec(paramsChunk))) {
			returnVal.macroParamsDictionary[paramMatch[1]] = paramMatch[2];
		}

		return returnVal;
	}

	/**
	 * generates the syntax for inserting a macro into a rich text editor - this is the very old umbraco style syntax     *
	 * @param {MacroSyntaxData} args an object containing the macro alias and it's parameter values
	 */
	generateMacroSyntax(args: MacroSyntaxData) {
		let macroString = `<?UMBRACO_MACRO macroAlias="${args.macroAlias}" `;

		for (const [key, val] of Object.entries(args.macroParamsDictionary)) {
			//check for null
			const valOrEmpty = val ?? '';
			//need to detect if the val is a string or an object
			let keyVal;
			if (typeof valOrEmpty === 'string') {
				keyVal = `${key}="${valOrEmpty}"`;
			} else {
				//if it's not a string we'll send it through the json serializer
				const json = JSON.parse(valOrEmpty);
				//then we need to url encode it so that it's safe
				const encoded = encodeURIComponent(json);
				keyVal = `${key}="${encoded}"`;
			}

			macroString += keyVal;
		}

		macroString += '/>';

		return macroString;
	}

	/**
	 * generates the syntax for inserting a macro into an mvc template     *
	 * @param {object} args an object containing the macro alias and it's parameter values
	 */
	generateMvcSyntax(args: MacroSyntaxData) {
		let macroString = `@await Umbraco.RenderMacroAsync("${args.macroAlias}"`;
		let hasParams = false;
		let paramString = '';

		if (args.macroParamsDictionary) {
			paramString = ', new {';

			for (const [key, val] of Object.entries(args.macroParamsDictionary)) {
				hasParams = true;

				const keyVal = `${key}="${val ? val : ''}", `;

				paramString += keyVal;
			}

			//remove the last , and trailing whitespace
			paramString = paramString.trimEnd().replace(/,*$/, '');
			paramString += '}';
		}

		if (hasParams) {
			macroString += paramString;
		}

		macroString += ')';

		return macroString;
	}

	collectValueData(macro: any, macroParams: any, renderingEngine: any) {
		const macroParamsDictionary: { [key: string]: string } = {};
		const macroAlias = macro.alias;
		if (!macroAlias) {
			throw 'The macro object does not contain an alias';
		}

		macroParams.forEach((item: any) => {
			let val = item.value;
			if (item.value !== null && item.value !== undefined && typeof item.value !== 'string') {
				try {
					val = JSON.parse(val);
				} catch (e) {
					// not json
				}
			}

			//each value needs to be xml escaped!! since the value get's stored as an xml attribute
			macroParamsDictionary[item.alias] = encodeURIComponent(val);
		});

		let syntax;

		//get the syntax based on the rendering engine
		if (renderingEngine && renderingEngine.toLowerCase() === 'mvc') {
			syntax = this.generateMvcSyntax({ macroAlias, macroParamsDictionary });
		} else {
			syntax = this.generateMacroSyntax({ macroAlias, macroParamsDictionary });
		}

		return {
			macroParamsDictionary,
			macroAlias,
			syntax,
		} as MacroSyntaxData;
	}
}

export const UMB_MACRO_SERVICE_CONTEXT_TOKEN = new UmbContextToken<UmbMacroService>(UmbMacroService.name);
