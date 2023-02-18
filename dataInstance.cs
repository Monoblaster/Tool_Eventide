$DataInstance::FilePath = "Config/Server/DataInstance";

function DataInstance_ListDelete(%list)
{
	%count = getWordCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		%curr = getWord(%list,%i);
		if(isObject(%curr))
		{
			%curr.delete();
		}
	}
}

function DataInstance_ListStringSerialize(%list)
{
	%curr = getWord(%list, 0);
	%s = "";
	if(isObject(%curr))
	{
		%s = %curr.StringSerialize();
	}
	%count = getWordCount(%list);
	for(%i = 1; %i < %count; %i++)
	{
		%curr = getWord(%list, %i);
		%serialized = "";
		if(isObject(%curr))
		{
			%serialized = %curr.StringSerialize();
		}
		%s = %s NL %serialized;
	}
	return %s;
}

function SimObject::DataInstance(%obj,%slot)
{
	%d = getWord(%obj.DataInstance_List,%slot);
	if(%d $= "")
	{
		%d = new ScriptObject(){class = "DataInstance";DataInstance_Parent = %obj;};
		%obj.DataInstance_List = setWord(%obj.DataInstance_List,%slot,%d);
	}
	return %d;
}

function SimObject::DataInstance_Set(%obj,%slot,%s)
{
	%s = getWord(%s,0);
	%obj.DataInstance_List = setWord(%obj.DataInstance_List,%slot,%s);
	return "";
}

function SimObject::DataInstance_Add(%obj,%s)
{
	%slot = getWordCount(%obj.DataInstance_List);
	%s = getWord(%s,0);
	%obj.DataInstance_List = setWord(%obj.DataInstance_List,%slot,%s);
	return "";
}

function SimObject::DataInstance_ListSet(%obj,%s)
{
	%obj.DataInstance_List = %s;
	return "";
}

function SimObject::DataInstance_ListSave(%obj)
{
	%id = %obj.DataIdentifier();
	%fo = new FileObject();
	%s = DataInstance_ListStringSerialize(%obj.DataInstance_List);
	%fo.close();
	if(%fo.OpenForWrite($DataInstance::FilePath @ "/" @ %id @ ".cs"))
	{
		%fo.writeLine(%s);
	}
	%fo.close();
	%fo.delete();
	return %id;
}

function SimObject::DataInstance_ListLoad(%obj)
{
	%id = %obj.DataIdentifier();
	%fo = new FileObject();
	if(%fo.OpenForRead($DataInstance::FilePath @ "/" @ %id @ ".cs"))
	{
		%c = 0;
		while(!%fo.isEOF())
		{
			%data[%c] = eval(%fo.readLine());
			%c++;
		}
	}
	%fo.close();
	%fo.delete();

	DataInstance_ListDelete(%obj.DataInstance_List);
	//unwrapped first loop to ensure propper formatting without trimming
	%data = %data[0];
	%obj.DataInstance_List = %data;
	if(isObject(%data))
	{
		%data.DataInstance_parent = %obj;
		if(%data.DataInstance_List !$= "")
		{
			%data.DataInstance_ListLoad();
		}
	}
	
	for(%i = 1; %i < %c; %i++)
	{
		%data = %data[%i];
		%obj.DataInstance_List = %obj.DataInstance_List SPC %data;
		if(isObject(%data))
		{
			%data.DataInstance_parent = %obj;
			if(%data.DataInstance_List !$= "")
			{
				%data.DataInstance_ListLoad();
			}
		}
	}
}


function Player::DataIdentifier(%obj,%append)
{
	%c = %obj.client;
	if(isObject(%c))
	{
		return %c.getBLID() @ %append;
	}
}

function DataInstance::DataIdentifier(%obj,%append)
{
	%parent = %obj.DataInstance_Parent;
	%list = %parent.DataInstance_List;
	%count = getWordCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		if(getWord(%list,%i) == %Obj)
		{
			return %parent.DataIdentifier("DataInstance" @ %i) @ %append;
		}
	}
	return "";
}

function DataInstance::StringSerialize(%d)
{
	%s = "new" SPC %d.getClassName() @ "(){class = \"DataInstance\";";
	%c = 0;
	while((%field = %d.getTaggedField(%c)) !$= "")
	{
		%name = getField(%field,0);
		%value = getField(%field,1);
		if(%name $= "DataInstance_List")
		{
			%value = %d.DataInstance_ListSave();
		}
		%s = %s @ %name @ "=\"" @ %value @ "\";";
		%c++;
	}
	%s = %s @ "};";
	return %s;
}

function DataInstance_GetFromThrower(%item)
{
    %p = findClientByBl_Id(%item.bl_id).player;
    if(isObject(%p))
    {
		%datablock = %item.getDatablock().getId();
		%list = %p.dataInstance_List;
        %count = getWordCount(%list);
        for(%i = 0; %i < %count; %i++)
        {
            if(%p.tool[%i] == 0 && isObject(%d = getWord(%list,%i)))
            {
                %item.dataInstance_set(0,%d);
				%p.DataInstance_Set(%i);
                return "";
            }
        }
    }
}

package DataInstance
{
	function Armor::OnRemove(%data,%obj)
	{
		DataInstance_ListDelete(%obj.DataInstance_List);
		parent::OnRemove(%data,%obj);
	}

	function Item::OnRemove(%data,%obj)
	{
	 	DataInstance_ListDelete(%obj.DataInstance_List);
		parent::OnRemove(%data,%obj);
	}

	function fxDtsBrick::OnRemove(%data,%obj)
	{
	 	DataInstance_ListDelete(%obj.DataInstance_List);
		parent::OnRemove(%data,%obj);
	}

	function DataInstance::OnRemove(%obj)
	{
	 	DataInstance_ListDelete(%obj.DataInstance_List);
	}

	function ItemData::onPickup (%this, %obj, %user, %amount)
    {
        //sigh looks like i have to play "find the difference"
        %maxTools = %user.getDatablock().maxTools;
        for(%i = 0; %i < %maxTools; %i++)
        {
            %before[%i] = %user.tool[%i];
        }
		%data = %obj.dataInstance(0);
		%obj.DataInstance_ListSet("");
        %r = parent::onPickup(%this, %obj, %user, %amount);

		for(%i = 0; %i < %maxTools; %i++)
		{
			if(%before[%i] != %user.tool[%i])
			{
				%user.DataInstance_set(%i,%data);
				break;
			}
		}
        
        return %r;
    }

    function ItemData::OnAdd(%db, %obj)
    {
    	schedule(0,%obj,"DataInstance_GetFromThrower",%obj);
        return Parent::OnAdd(%db, %obj);
    }
};
activatePackage("DataInstance");