export function incrementString(text: string) {
	return text.replace(/(\d*)$/, (_, t) => (+t + 1).toString().padStart(t.length, '0'));
}
