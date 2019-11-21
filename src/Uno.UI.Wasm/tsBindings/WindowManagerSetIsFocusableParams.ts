/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
class WindowManagerSetIsFocusableParams
{
	/* Pack=4 */
	HtmlId : number;
	IsFocusable : boolean;
	public static unmarshal(pData:number) : WindowManagerSetIsFocusableParams
	{
		let ret = new WindowManagerSetIsFocusableParams();
		
		{
			ret.HtmlId = Number(Module.getValue(pData + 0, "*"));
		}
		
		{
			ret.IsFocusable = Boolean(Module.getValue(pData + 4, "i32"));
		}
		return ret;
	}
}
